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
            var name = input.ActivityName;
            var value = input.Value;
            var options = GetTaskOptions();

            var result = await context.CallActivityAsync<OrchestrationActivityResult<TOut>>(name, value, options);

            if (result.IsSuccess)
            {
                return new OrchestrationActivityCallOut<TOut>(result.Value);
            }

            return Failure.Create(HandlerFailureCode.Persistent, result.FailureMessage);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ex.ToFailure(
                HandlerFailureCode.Transient,
                $"An unexpected exception was thrown when trying to call a task '{input.ActivityName}'");
        }
    }
}