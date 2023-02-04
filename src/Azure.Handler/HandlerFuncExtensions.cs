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
                HandlerFailureAction.Retry => throw new InvalidOperationException(failure.FailureMessage),
                _ => Unit.Invoke(OnRemoveMessage<THandler, THandlerData>, context, handlerData, failure.FailureMessage)
            };
    }

    private static void OnRemoveMessage<THandler, THandlerData>(
        FunctionContext context, THandlerData handlerData, string message)
    {
        var logger = context.GetLogger(typeof(THandler).Name);
        logger.LogWarning("Data will not be retried: {data}. Error: {error}", handlerData, message);

        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "RemoveMessage",
            new Dictionary<string, string>
            {
                ["data"] = handlerData?.ToString() ?? string.Empty,
                ["message"] = message
            });
    }
}