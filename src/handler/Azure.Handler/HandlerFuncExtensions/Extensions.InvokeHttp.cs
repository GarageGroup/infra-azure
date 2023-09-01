using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, cancellationToken);

#if NET7_0_OR_GREATER
        var json = await request.Body.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        var json = await request.Body.ReadAsStringAsync().ConfigureAwait(false);
#endif

        var result = await json.DeserializeOrFailure<TIn>().ForwardValueAsync(handler.HandleOrFailureAsync, tokenSource.Token).ConfigureAwait(false);
        return result.Fold(request.CreateSuccessResponse, InnerCreateFailureResponse);

        HttpResponseData InnerCreateFailureResponse(Failure<HandlerFailureCode> failure)
        {
            var code = failure.FailureCode is HandlerFailureCode.Transient ? "transient" : "persistent";

            context.GetFunctionLogger().LogError(
                failure.SourceException,
                "An unexpected {code} HTTP Function error occured: {error}", code, failure.FailureMessage);

            context.TrackHandlerFailure(failure, json);

            if (failure.FailureCode is HandlerFailureCode.Transient)
            {
                return request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return request.CreatePersistentFailureResponse(failure);
        }
    }

    private static HttpResponseData CreateSuccessResponse<T>(this HttpRequestData httpRequest, T success)
    {
        Debug.Assert(httpRequest is not null);

        if (success is IHttpResponseProvider httpResponseProvider)
        {
            return httpResponseProvider.GetHttpResponse(httpRequest);
        }

        if (success is Unit)
        {
            return httpRequest.CreateResponse(HttpStatusCode.NoContent);
        }

        var json = JsonSerializer.Serialize(success, SerializerOptions);
        var response = httpRequest.CreateResponse(HttpStatusCode.OK);

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