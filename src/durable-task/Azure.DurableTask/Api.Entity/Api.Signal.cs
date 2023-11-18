using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DurableTask.Entities;

namespace GarageGroup.Infra;

partial class OrchestrationEntityApi
{
    public ValueTask<Result<Unit, Failure<HandlerFailureCode>>> SignalEntityAsync<TIn>(
        OrchestrationEntitySignalIn<TIn> input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, Failure<HandlerFailureCode>>>(cancellationToken);
        }

        return InnerSignalEntityAsync(input, cancellationToken);
    }

    private async ValueTask<Result<Unit, Failure<HandlerFailureCode>>> InnerSignalEntityAsync<TIn>(
        OrchestrationEntitySignalIn<TIn> input, CancellationToken cancellationToken)
    {
        var operation = input.OperationName;

        try
        {
            var id = new EntityInstanceId(
                name: input.Entity.Name, 
                key: input.Entity.Key);

            await durableTaskClient.Entities.SignalEntityAsync(id, input.OperationName, input.Value, null, cancellationToken).ConfigureAwait(false);

            return Result.Success<Unit>(default);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var entityText = $"{input.Entity.Name}/{input.Entity.Key}";

            return ex.ToFailure(
                failureCode: HandlerFailureCode.Transient,
                failureMessage: $"An unexpected exception was thrown when trying to call operation '{operation}' with an entity '{entityText}'");
        }
    }
}
