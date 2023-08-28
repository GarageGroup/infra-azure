using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DurableTask;

namespace GarageGroup.Infra;

partial class OrchestrationInstanceApi
{
    public ValueTask<Result<OrchestrationInstanceScheduleOut, Failure<HandlerFailureCode>>> ScheduleInstanceAsync<TIn>(
        OrchestrationInstanceScheduleIn<TIn> input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<OrchestrationInstanceScheduleOut, Failure<HandlerFailureCode>>>(cancellationToken);
        }

        return InnerScheduleInstanceAsync(input, cancellationToken);
    }

    private async ValueTask<Result<OrchestrationInstanceScheduleOut, Failure<HandlerFailureCode>>> InnerScheduleInstanceAsync<TIn>(
        OrchestrationInstanceScheduleIn<TIn> input, CancellationToken cancellationToken)
    {
        try
        {
            var option = new StartOrchestrationOptions
            {
                InstanceId = input.InstanceId.OrNullIfEmpty(),
                StartAt = input.StartAt
            };

            var instanceId = await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                input.OrchestratorName, option, cancellationToken);

            return new OrchestrationInstanceScheduleOut(
                instanceId: instanceId.OrEmpty());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ex.ToFailure(
                HandlerFailureCode.Transient,
                $"An unexpected exception was thrown when trying to schedule a task '{input.OrchestratorName}'");
        }
    }
}