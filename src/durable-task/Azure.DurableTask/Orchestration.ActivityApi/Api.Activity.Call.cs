using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

partial class OrchestrationActivityApi
{
    public ValueTask<Result<Unit, Failure<HandlerFailureCode>>> CallActivityAsync<TIn>(
        OrchestrationActivityCallIn<TIn> input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, Failure<HandlerFailureCode>>>(cancellationToken);
        }

        return InnerCallActivityAsync(input);
    }

    private async ValueTask<Result<Unit, Failure<HandlerFailureCode>>> InnerCallActivityAsync<TIn>(
        OrchestrationActivityCallIn<TIn> input)
    {
        try
        {
            var name = input.ActivityName;
            var value = input.Value;
            var options = GetTaskOptions();

            var result = await context.CallActivityAsync<OrchestrationActivityResult>(name, value, options);

            if (result.IsSuccess)
            {
                return Result.Success<Unit>(default);
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