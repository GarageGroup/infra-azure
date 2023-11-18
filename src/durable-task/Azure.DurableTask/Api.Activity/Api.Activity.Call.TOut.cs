using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

partial class OrchestrationActivityApi
{
    public ValueTask<Result<OrchestrationActivityCallOut<TOut>, Failure<HandlerFailureCode>>> CallActivityAsync<TIn, TOut>(
        OrchestrationActivityCallIn<TIn> input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<OrchestrationActivityCallOut<TOut>, Failure<HandlerFailureCode>>>(cancellationToken);
        }

        return InnerCallActivityAsync<TIn, TOut>(input);
    }

    private async ValueTask<Result<OrchestrationActivityCallOut<TOut>, Failure<HandlerFailureCode>>> InnerCallActivityAsync<TIn, TOut>(
        OrchestrationActivityCallIn<TIn> input)
    {
        try
        {
            var result = await context.CallActivityAsync<HandlerResultJson<TOut>>(input.ActivityName, input.Value, GetTaskOptions());
            return result.ToResult().MapSuccess(InnerCreateOutput);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return CreateTransientHandlerFailure(ex, input.ActivityName);
        }

        static OrchestrationActivityCallOut<TOut> InnerCreateOutput(TOut? @out)
            =>
            new(@out);
    }
}