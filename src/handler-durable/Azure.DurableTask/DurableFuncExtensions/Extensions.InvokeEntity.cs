using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

partial class DurableFuncExtensions
{
    public static Task<TOut> InvokeEntityFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        TaskEntityDispatcher dispatcher,
        FunctionContext functionContext,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(functionContext);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TOut>(cancellationToken);
        }

        return handler.InternalInvokeEntityFunctionAsync<THandler, TIn, TOut>(dispatcher, functionContext, cancellationToken);
    }

    internal static async Task<TOut> InternalInvokeEntityFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        TaskEntityDispatcher dispatcher,
        FunctionContext functionContext,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        TOut? output = default;
        await dispatcher.DispatchAsync(InnerDispatchAsync);
        return output!;

        async ValueTask<object?> InnerDispatchAsync(TaskEntityOperation operation)
        {
            using var source = CancellationTokenSource.CreateLinkedTokenSource(functionContext.CancellationToken, cancellationToken);
            var result = await operation.GetInputOrFailure<TIn>().ForwardValueAsync(handler.HandleOrFailureAsync, source.Token);

            if (result.IsSuccess)
            {
                output = result.SuccessOrThrow();
                return default;
            }

            var failure = result.FailureOrThrow();

            functionContext.GetLogger(functionContext.FunctionDefinition.Name).LogError(
                failure.SourceException,
                "An entity function has failed. Error: {error}", failure.FailureMessage);

            functionContext.TrackHandlerFailure(failure);
            throw failure.ToException();
        }
    }
}