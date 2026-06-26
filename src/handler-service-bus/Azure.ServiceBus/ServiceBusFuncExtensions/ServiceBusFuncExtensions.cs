using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

internal static partial class ServiceBusFuncExtensions
{
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