using System;

namespace GarageGroup.Infra;

public sealed record class OrchestrationActivityApiOption
{
    public OrchestrationActivityApiOption(int maxNumberOfAttempts, TimeSpan firstRetryInterval, double backoffCoefficient = 1)
    {
        MaxNumberOfAttempts = maxNumberOfAttempts;
        FirstRetryInterval = firstRetryInterval;
        BackoffCoefficient = backoffCoefficient;
    }

    public int MaxNumberOfAttempts { get; }

    public TimeSpan FirstRetryInterval { get; }

    public double BackoffCoefficient { get; }

    public TimeSpan? MaxRetryInterval { get; init; }

    public TimeSpan? RetryTimeout { get; init; }
}