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
            return exception.CreateDeserializerFailure();
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
            return exception.CreateDeserializerFailure();
        }
    }

    private static Failure<HandlerFailureCode> CreateDeserializerFailure(this Exception exception)
        =>
        new(HandlerFailureCode.Persistent, $"An unexpected error occured when the request body was being deserialized: '{exception.Message}'");

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

    private static async Task<string> ReadAsStringAsync(this Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return await streamReader.ReadToEndAsync().ConfigureAwait(false) ?? string.Empty;
    }

    private static void TrackPersistentFailure(this FunctionContext context, string requestData, string message)
        =>
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "PersistentFailure",
            new Dictionary<string, string>
            {
                ["function"] = context.FunctionDefinition.Name,
                ["data"] = requestData,
                ["message"] = message
            });

    private static void TrackTransientFailure(this FunctionContext context, string requestData, string message)
        =>
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "TransientFailure",
            new Dictionary<string, string>
            {
                ["function"] = context.FunctionDefinition.Name,
                ["data"] = requestData,
                ["message"] = message
            });

    private static void TrackException(this FunctionContext context, string requestData, Exception exception)
        =>
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "TransientFailure",
            new Dictionary<string, string>
            {
                ["function"] = context.FunctionDefinition.Name,
                ["data"] = requestData,
                ["message"] = exception.Message,
                ["errorType"] = exception.GetType().FullName ?? string.Empty,
                ["stackTrace"] = exception.StackTrace ?? string.Empty
            });
}