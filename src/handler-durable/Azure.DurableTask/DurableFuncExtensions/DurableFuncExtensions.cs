using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

namespace GarageGroup.Infra;

public static partial class DurableFuncExtensions
{
    private static Result<TIn?, Failure<HandlerFailureCode>> GetInputOrFailure<TIn>(
        this TaskOrchestrationContext orchestrationContext)
    {
        try
        {
            return orchestrationContext.GetInput<TIn>();
        }
        catch (Exception exception)
        {
            return exception.ToFailure(
                HandlerFailureCode.Persistent, $"An unexpected exception was thrown when trying to get orchestration input of {typeof(TIn).FullName}");
        }
    }

    private static Result<TIn?, Failure<HandlerFailureCode>> GetInputOrFailure<TIn>(
        this TaskEntityOperation entityOperation)
    {
        try
        {
            return entityOperation.GetInput<TIn>();
        }
        catch (Exception exception)
        {
            return exception.ToFailure(
                HandlerFailureCode.Persistent, $"An unexpected exception was thrown when trying to get entity input of {typeof(TIn).FullName}");
        }
    }

    private static async ValueTask<Result<TOut, Failure<HandlerFailureCode>>> HandleOrFailureAsync<TIn, TOut>(
        this IHandler<TIn, TOut> handler, TIn? input, CancellationToken cancellationToken)
    {
        try
        {
            return await handler.HandleAsync(input, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return exception.ToFailure(HandlerFailureCode.Transient, "An unexpected exception was thrown in the handler");
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
}