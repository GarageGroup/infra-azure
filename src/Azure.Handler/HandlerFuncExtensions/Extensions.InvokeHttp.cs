using System;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

partial class HandlerFuncExtensions
{
    public static Task<HttpResponseData> InvokeHttpFunctionAsync<THandler, TIn, TOut>(
        this THandler handler, HttpRequestData request, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<HttpResponseData>(cancellationToken);
        }

        return handler.InternalHttpFunctionAsync<THandler, TIn, TOut>(request, cancellationToken);
    }

    internal static async Task<HttpResponseData> InternalHttpFunctionAsync<THandler, TIn, TOut>(
        this THandler handler, HttpRequestData request, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        var context = request.FunctionContext;

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(request.FunctionContext.CancellationToken, cancellationToken);
        var json = await request.Body.ReadAsStringAsync().ConfigureAwait(false);

        try
        {
            var result = await json.DeserializeOrFailure<TIn>().ForwardValueAsync(handler.HandleAsync, tokenSource.Token).ConfigureAwait(false);
            return result.Fold(InnerCreateSuccessResponse, InnerCreateFailureResponse);
        }
        catch (Exception ex)
        {
            InnerLogException(request.FunctionContext, json, ex);
            throw;
        }

        HttpResponseData InnerCreateSuccessResponse(TOut @out)
            =>
            request.CreateSuccessResponse(@out);

        HttpResponseData InnerCreateFailureResponse(Failure<HandlerFailureCode> failure)
            =>
            failure.FailureCode switch
            {
                HandlerFailureCode.Transient    => InnerCreateTransientFailureResponse(failure),
                _                               => InnerCreatePersistentFailureResponse(failure)
            };

        HttpResponseData InnerCreatePersistentFailureResponse(Failure<HandlerFailureCode> failure)
        {
            var message = failure.FailureMessage;

            context.GetLogger(context.FunctionDefinition.Name).LogError("An unexpected persistent HTTP Function error occured: {error}", message);
            context.TrackTransientFailure(json, message);

            return request.CreatePersistentFailureResponse(failure);
        }

        HttpResponseData InnerCreateTransientFailureResponse(Failure<HandlerFailureCode> failure)
        {
            var message = failure.FailureMessage;

            context.GetLogger(context.FunctionDefinition.Name).LogError("An unexpected transient HTTP Function error occured: {error}", message);
            context.TrackTransientFailure(json, message);

            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        static void InnerLogException(FunctionContext context, string requestData, Exception exception)
        {
            context.GetLogger(context.FunctionDefinition.Name).LogError(exception, "An unexpected HTTP Function exception was thrown");
            context.TrackException(requestData.ToString(), exception);
        }
    }

    private static HttpResponseData CreateSuccessResponse<TOut>(this HttpRequestData request, TOut success)
    {
        if (success is Unit)
        {
            return request.CreateResponse(HttpStatusCode.NoContent);
        }

        var json = JsonSerializer.Serialize(success, SerializerOptions);
        var response = request.CreateResponse(HttpStatusCode.OK);

        response.WriteString(json);
        response.Headers.TryAddWithoutValidation("Content-Type", MediaTypeNames.Application.Json);

        return response;
    }

    private static HttpResponseData CreatePersistentFailureResponse(this HttpRequestData request, Failure<HandlerFailureCode> failure)
    {
        var httpFailure = new HttpFailureJson
        {
            Type = "Bad Request",
            Title = "about:blank",
            Status = 400,
            Detail = failure.FailureMessage
        };

        var failureJson = JsonSerializer.Serialize(httpFailure, SerializerOptions);
        var response = request.CreateResponse(HttpStatusCode.BadRequest);

        response.WriteString(failureJson);
        response.Headers.TryAddWithoutValidation("Content-Type", "application/problem+json");

        return response;
    }

    private sealed record class HttpFailureJson
    {
        public string? Type { get; init; }

        public string? Title { get; init; }

        public int Status { get; init; }

        public string? Detail { get; init; }
    }
}