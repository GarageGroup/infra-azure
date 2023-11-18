using System;
using Microsoft.DurableTask;

namespace GarageGroup.Infra;

internal sealed partial class OrchestrationActivityApi : IOrchestrationActivityApi
{
    private readonly TaskOrchestrationContext context;

    private readonly OrchestrationActivityApiOption? option;

    internal OrchestrationActivityApi(TaskOrchestrationContext context, OrchestrationActivityApiOption? option)
    {
        this.context = context;
        this.option = option;
    }

    private TaskOptions? GetTaskOptions()
    {
        if (option is null)
        {
            return null;
        }

        return new TaskOptions(
            retry: new RetryPolicy(
                maxNumberOfAttempts: option.MaxNumberOfAttempts,
                firstRetryInterval: option.FirstRetryInterval,
                backoffCoefficient: option.BackoffCoefficient,
                maxRetryInterval: option.MaxRetryInterval,
                retryTimeout: option.RetryTimeout));
    }

    private static Failure<HandlerFailureCode> CreateTransientHandlerFailure(Exception exception, string activityName)
        =>
        exception.ToFailure(HandlerFailureCode.Transient, $"An unexpected exception was thrown when trying to call a task '{activityName}'");
}