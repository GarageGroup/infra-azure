using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

partial class HandlerFuncExtensions
{
    public static Task InvokeAzureFunctionAsync<THandler, TIn, TOut>(
        this THandler handler, JsonElement jsonData, FunctionContext context, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(context);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        return handler.InternalInvokeAzureFunctionAsync<THandler, TIn, TOut>(jsonData, context, cancellationToken);
    }

    internal static async Task InternalInvokeAzureFunctionAsync<THandler, TIn, TOut>(
        this THandler handler, JsonElement jsonData, FunctionContext context, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, cancellationToken);

        var result = await InnerInvokeAsync(tokenSource.Token).ConfigureAwait(false);
        _ = result.Fold(Unit.From, OnFailure);

        async ValueTask<Result<TOut, Failure<HandlerFailureCode>>> InnerInvokeAsync(CancellationToken token)
        {
            try
            {
                var handlerData = jsonData.Deserialize<TIn>(SerializerOptions);
                return await handler.HandleAsync(handlerData, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogException(context, jsonData, ex);
                throw;
            }
        }

        Unit OnFailure(Failure<HandlerFailureCode> failure)
            =>
            failure.FailureCode switch
            {
                HandlerFailureCode.Transient    => Unit.Invoke(OnTransientFailure, context, jsonData, failure.FailureMessage),
                _                               => Unit.Invoke(OnPersistentFailure, context, jsonData, failure.FailureMessage)
            };

        static void OnPersistentFailure(FunctionContext context, JsonElement jsonData, string message)
        {
            context.GetLogger(context.FunctionDefinition.Name).LogError("Data will be removed: {data}. Error: {error}", jsonData, message);
            context.TrackPersistentFailure(jsonData.ToString(), message);
        }

        static void OnTransientFailure(FunctionContext context, JsonElement jsonData, string message)
        {
            context.GetLogger(context.FunctionDefinition.Name).LogError("Data will be retried: {data}. Error: {error}", jsonData, message);
            context.TrackTransientFailure(jsonData.ToString(), message);

            throw new InvalidOperationException(message);
        }

        static void LogException(FunctionContext context, JsonElement jsonData, Exception exception)
        {
            context.GetLogger(context.FunctionDefinition.Name).LogError(exception, "Data will be retried: {data} due to exception", jsonData);
            context.TrackException(jsonData.ToString(), exception);
        }
    }
}