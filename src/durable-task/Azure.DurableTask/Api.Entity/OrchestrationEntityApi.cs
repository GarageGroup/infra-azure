using Microsoft.DurableTask.Client;

namespace GarageGroup.Infra;

internal sealed partial class OrchestrationEntityApi : IOrchestrationEntityApi
{
    private readonly DurableTaskClient durableTaskClient;

    internal OrchestrationEntityApi(DurableTaskClient durableTaskClient)
        =>
        this.durableTaskClient = durableTaskClient;
}