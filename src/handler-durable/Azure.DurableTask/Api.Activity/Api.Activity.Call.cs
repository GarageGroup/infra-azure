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
            var result = await context.CallActivityAsync<HandlerResultJson<Unit>>(input.ActivityName, input.Value, GetTaskOptions());
            return result.ToResult();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return CreateTransientHandlerFailure(ex, input.ActivityName);
        }
    }
}