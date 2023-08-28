using Microsoft.DurableTask.Client;

namespace GarageGroup.Infra;

internal sealed partial class OrchestrationInstanceApi : IOrchestrationInstanceApi
{
    private readonly DurableTaskClient durableTaskClient;

    internal OrchestrationInstanceApi(DurableTaskClient durableTaskClient)
        =>
        this.durableTaskClient = durableTaskClient;
}