using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator]
public sealed class DurableHandlerFunctionSourceGenerator : HandlerFunctionSourceGeneratorBase
{
    private static readonly IReadOnlyList<IFunctionDataProvider> DataProviders;

    static DurableHandlerFunctionSourceGenerator()
        =>
        DataProviders = new IFunctionDataProvider[]
        {
            new ActivityFunctionDataProvider(),
            new EntityFunctionDataProvider(),
            new OrchestrationFunctionDataProvider()
        };

    protected override HandlerFunctionProvider GetFunctionProvider()
        =>
        new(DataProviders, "DurableHandlerFunction");
}