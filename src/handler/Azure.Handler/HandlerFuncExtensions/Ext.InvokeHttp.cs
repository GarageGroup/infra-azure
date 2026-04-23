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

        return handler.InternalHttpFunctionAsync<THandler, TIn, TOut>(
            request: request,
            readInputFunc: default,
            createSuccessResponseFunc: default,
            createFailureResponseFunc: default,
            cancellationToken: cancellationToken);
    }

    public static Task<HttpResponseData> InvokeHttpFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        HttpRequestData request,
        Func<HttpRequestData, string, Result<TIn?, Failure<HandlerFailureCode>>>? readInputFunc,
        Func<HttpRequestData, TOut, HttpResponseData>? createSuccessResponseFunc,
        Func<HttpRequestData, Failure<HandlerFailureCode>, HttpResponseData>? createFailureResponseFunc,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(request);

        return handler.InternalHttpFunctionAsync(
            request: request,
            readInputFunc: readInputFunc,
            createSuccessResponseFunc: createSuccessResponseFunc,
            createFailureResponseFunc: createFailureResponseFunc,
            cancellationToken: cancellationToken);
    }

    internal static Task<HttpResponseData> InternalHttpFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        HttpRequestData request,
        Func<HttpRequestData, string, Result<TIn?, Failure<HandlerFailureCode>>>? readInputFunc,
        Func<HttpRequestData, TOut, HttpResponseData>? createSuccessResponseFunc,
        Func<HttpRequestData, Failure<HandlerFailureCode>, HttpResponseData>? createFailureResponseFunc,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        return handler.InnerHttpFunctionAsync(
            request: request,
            readInputFunc: readInputFunc ?? InnerDeserializeInput,
            createSuccessResponseFunc: createSuccessResponseFunc ?? CreateSuccessResponse,
            createFailureResponseFunc: createFailureResponseFunc ?? CreateFailureResponse,
            cancellationToken: cancellationToken);

        static Result<TIn?, Failure<HandlerFailureCode>> InnerDeserializeInput(
            HttpRequestData _, string requestBody)
        {
            if (string.IsNullOrEmpty(requestBody))
            {
                return Result.Success<TIn?>(default);
            }

            return JsonSerializer.Deserialize<TIn>(requestBody, SerializerOptions);
        }
    }

    private static async Task<HttpResponseData> InnerHttpFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        HttpRequestData request,
        Func<HttpRequestData, string, Result<TIn?, Failure<HandlerFailureCode>>> readInputFunc,
        Func<HttpRequestData, TOut, HttpResponseData> createSuccessResponseFunc,
        Func<HttpRequestData, Failure<HandlerFailureCode>, HttpResponseData> createFailureResponseFunc,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        var context = request.FunctionContext;
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, cancellationToken);

        var requestBody = await request.Body.ReadAsStringAsync(cancellationToken);

        var result = await InnerReadInputOrFailure().ForwardValueAsync(handler.HandleOrFailureAsync, tokenSource.Token);
        return result.Fold(InnerCreateSuccessResponse, InnerCreateFailureResponse);

        Result<TIn?, Failure<HandlerFailureCode>> InnerReadInputOrFailure()
        {
            try
            {
                return readInputFunc.Invoke(request, requestBody);
            }
            catch (Exception exception)
            {
                return exception.ToFailure(
                    HandlerFailureCode.Persistent, "An unexpected error occured when the request body was being deserialized");
            }
        }

        HttpResponseData InnerCreateSuccessResponse(TOut success)
            =>
            createSuccessResponseFunc.Invoke(request, success);

        HttpResponseData InnerCreateFailureResponse(Failure<HandlerFailureCode> failure)
        {
            var code = failure.FailureCode is HandlerFailureCode.Transient ? "transient" : "persistent";

            context.GetFunctionLogger().LogError(
                failure.SourceException,
                "An unexpected {code} HTTP Function error occured: {error}", code, failure.FailureMessage);

            context.TrackHandlerFailure(failure, requestBody);
            return createFailureResponseFunc.Invoke(request, failure);
        }
    }

    private static HttpResponseData CreateSuccessResponse<T>(HttpRequestData httpRequest, T success)
    {
        Debug.Assert(httpRequest is not null);

        if (success is IHttpResponseProvider httpResponseProvider)
        {
            return httpResponseProvider.GetHttpResponse(httpRequest);
        }

        if (success is Unit || success is null)
        {
            return httpRequest.CreateResponse(HttpStatusCode.NoContent);
        }

        var response = httpRequest.CreateResponse(HttpStatusCode.OK);
        if (success is string text)
        {
            response.WriteString(text);
            return response;
        }

        var json = JsonSerializer.Serialize(success, SerializerOptions);

        response.WriteString(json);
        response.Headers.TryAddWithoutValidation("Content-Type", MediaTypeNames.Application.Json);

        return response;
    }

    private static HttpResponseData CreateFailureResponse(HttpRequestData request, Failure<HandlerFailureCode> failure)
    {
        var response = request.CreateResponse();

        response.StatusCode = failure.FailureCode is HandlerFailureCode.Persistent ? HttpStatusCode.BadRequest : HttpStatusCode.InternalServerError;
        response.WriteString(failure.FailureMessage);

        return response;
    }
}