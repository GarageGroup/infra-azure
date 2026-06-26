using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator]
public sealed class ServiceBusHandlerFunctionSourceGenerator : HandlerFunctionSourceGeneratorBase
{
    private static readonly IReadOnlyList<IFunctionDataProvider> DataProviders;

    static ServiceBusHandlerFunctionSourceGenerator()
        =>
        DataProviders =
        [
            new ServiceBusFunctionDataProvider()
        ];

    protected override HandlerFunctionProvider GetFunctionProvider()
        =>
        new(DataProviders, "ServiceBusHandlerFunction");
}