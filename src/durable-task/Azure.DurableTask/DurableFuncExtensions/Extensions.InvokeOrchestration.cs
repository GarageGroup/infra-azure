using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

partial class DurableFuncExtensions
{
    public static Task<TOut> InvokeOrchestrationFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        TaskOrchestrationContext orchestrationContext,
        FunctionContext functionContext,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(orchestrationContext);
        ArgumentNullException.ThrowIfNull(functionContext);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TOut>(cancellationToken);
        }

        return handler.InternalInvokeOrchestrationFunctionAsync<THandler, TIn, TOut>(orchestrationContext, functionContext, cancellationToken);
    }

    internal static async Task<TOut> InternalInvokeOrchestrationFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        TaskOrchestrationContext orchestrationContext,
        FunctionContext functionContext,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(functionContext.CancellationToken, cancellationToken);
        var result = await orchestrationContext.GetInputOrFailure<TIn>().ForwardValueAsync(handler.HandleOrFailureAsync, tokenSource.Token);

        return result.MapFailure(LogFailure).SuccessOrThrow(ToException);

        Failure<HandlerFailureCode> LogFailure(Failure<HandlerFailureCode> failure)
        {
            orchestrationContext.CreateReplaySafeLogger(functionContext.FunctionDefinition.Name).LogError(
                failure.SourceException,
                "An orchestration instance has failed. Error: {error}", failure.FailureMessage);

            functionContext.TrackHandlerFailure(failure);
            return failure;
        }

        static HandlerFailureException ToException(Failure<HandlerFailureCode> failure)
            =>
            failure.ToException();
    }
}