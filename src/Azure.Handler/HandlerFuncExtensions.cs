using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra;

public static class HandlerFuncExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions;

    static HandlerFuncExtensions()
        =>
        SerializerOptions = new(JsonSerializerDefaults.Web);

    public static Task InvokeAzureFunctionAsync<THandler, THandlerData>(
        this THandler handler, JsonElement jsonData, FunctionContext context)
        where THandler : IHandler<THandlerData>
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(context);

        return handler.InternalInvokeAzureFunctionAsync<THandler, THandlerData>(jsonData, context);
    }

    internal static async Task InternalInvokeAzureFunctionAsync<THandler, THandlerData>(
        this THandler handler, JsonElement jsonData, FunctionContext context)
        where THandler : IHandler<THandlerData>
    {
        var handlerData = jsonData.Deserialize<THandlerData>(SerializerOptions)!;
        var result = await handler.HandleAsync(handlerData, context.CancellationToken);

        _ = result.Fold(Unit.From, OnFailure);

        Unit OnFailure(HandlerFailure failure)
            =>
            failure.FailureAction switch
            {
                HandlerFailureAction.Retry  => Unit.Invoke(OnRetryMessage<THandler>, context, jsonData, failure.FailureMessage),
                _                           => Unit.Invoke(OnRemoveMessage<THandler>, context, jsonData, failure.FailureMessage)
            };
    }

    private static void OnRemoveMessage<THandler>(FunctionContext context, JsonElement jsonData, string message)
    {
        var logger = context.GetLogger(typeof(THandler).Name);
        logger.LogWarning("Data will not be retried: {data}. Error: {error}", jsonData, message);

        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "RemoveMessage",
            new Dictionary<string, string>
            {
                ["data"] = jsonData.ToString(),
                ["message"] = message
            });
    }

    private static void OnRetryMessage<THandler>(FunctionContext context, JsonElement jsonData, string message)
    {
        var logger = context.GetLogger(typeof(THandler).Name);
        logger.LogError("Data will be retried: {data}. Error: {error}", jsonData, message);

        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "RetryMessage",
            new Dictionary<string, string>
            {
                ["data"] = jsonData.ToString(),
                ["message"] = message
            });

        throw new InvalidOperationException(message);
    }
}