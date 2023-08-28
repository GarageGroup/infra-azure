using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

public static partial class HandlerFuncExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions;

    static HandlerFuncExtensions()
        =>
        SerializerOptions = new(JsonSerializerDefaults.Web);

    private static Result<T?, Failure<HandlerFailureCode>> DeserializeOrFailure<T>(this JsonElement json)
    {
        try
        {
            return json.Deserialize<T>(SerializerOptions);
        }
        catch (Exception exception)
        {
            return exception.ToFailure(
                HandlerFailureCode.Persistent, "An unexpected error occured when the request body was being deserialized");
        }
    }

    private static Result<T?, Failure<HandlerFailureCode>> DeserializeOrFailure<T>(this string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
        catch (Exception exception)
        {
            return exception.ToFailure(
                HandlerFailureCode.Persistent, "An unexpected error occured when the request body was being deserialized");
        }
    }

    private static async ValueTask<Result<TOut, Failure<HandlerFailureCode>>> HandleOrFailureAsync<TIn, TOut>(
        this IHandler<TIn, TOut> handler, TIn? input, CancellationToken cancellationToken)
    {
        try
        {
            return await handler.HandleAsync(input, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ex.ToFailure(HandlerFailureCode.Transient, "An unexpected exception was thrown in the handler");
        }
    }

    private static ValueTask<Result<TOut, Failure<HandlerFailureCode>>> ForwardValueAsync<TIn, TOut>(
        this Result<TIn, Failure<HandlerFailureCode>> source,
        Func<TIn, CancellationToken, ValueTask<Result<TOut, Failure<HandlerFailureCode>>>> nextAsync,
        CancellationToken cancellationToken)
    {
        return source.ForwardValueAsync(InnerInvokeAsync);

        ValueTask<Result<TOut, Failure<HandlerFailureCode>>> InnerInvokeAsync(TIn input)
            =>
            nextAsync.Invoke(input, cancellationToken);
    }

#if NET7_0_OR_GREATER
    private static async Task<string> ReadAsStringAsync(this Stream stream, CancellationToken cancellationToken)
#else
    private static async Task<string> ReadAsStringAsync(this Stream stream)
#endif
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);

#if NET7_0_OR_GREATER
        return await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false) ?? string.Empty;
#else
        return await streamReader.ReadToEndAsync().ConfigureAwait(false) ?? string.Empty;
#endif
    }

    private static ILogger GetFunctionLogger(this FunctionContext context)
        =>
        context.GetLogger(context.FunctionDefinition.Name);

    private static void TrackFailure(
        this FunctionContext context, Failure<HandlerFailureCode> failure, string requestData)
    {
        var properties = new Dictionary<string, string>
        {
            ["function"] = context.FunctionDefinition.Name,
            ["data"] = requestData,
            ["message"] = failure.FailureMessage
        };

        if (failure.SourceException is not null)
        {
            properties["errorMessage"] = failure.SourceException.Message ?? string.Empty;
            properties["errorType"] = failure.SourceException.GetType().FullName ?? string.Empty;
            properties["stackTrace"] = failure.SourceException.StackTrace ?? string.Empty;
        }

        var eventName = failure.FailureCode is HandlerFailureCode.Transient ? "TransientFailure" : "PersistentFailure";
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(eventName, properties);
    }
}