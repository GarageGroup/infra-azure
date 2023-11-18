using System;
using Microsoft.DurableTask.Client;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class OrchestrationEntityApiDependency
{
    public static Dependency<IOrchestrationEntityApi> UseOrchestrationEntityApi(
        this Dependency<DurableTaskClient> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IOrchestrationEntityApi>(CreateApi);

        static OrchestrationEntityApi CreateApi(DurableTaskClient durableTaskClient)
        {
            ArgumentNullException.ThrowIfNull(durableTaskClient);
            return new(durableTaskClient);
        }
    }
}
