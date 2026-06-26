using System;
using Microsoft.DurableTask.Client;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class OrchestrationInstanceApiDependency
{
    public static Dependency<IOrchestrationInstanceApi> UseOrchestrationInstanceApi(
        this Dependency<DurableTaskClient> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IOrchestrationInstanceApi>(CreateApi);

        static OrchestrationInstanceApi CreateApi(DurableTaskClient durableTaskClient)
        {
            ArgumentNullException.ThrowIfNull(durableTaskClient);
            return new(durableTaskClient);
        }
    }
}