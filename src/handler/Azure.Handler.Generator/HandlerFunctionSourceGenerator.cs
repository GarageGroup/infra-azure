using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator]
public sealed class HandlerFunctionSourceGenerator : HandlerFunctionSourceGeneratorBase
{
    private static readonly IReadOnlyList<IFunctionDataProvider> DataProviders;

    static HandlerFunctionSourceGenerator()
        =>
        DataProviders =
        [
            new HttpFunctionDataProvider(),
            new ServiceBusFunctionDataProvider(),
            new EventGridFunctionDataProvider()
        ];

    protected override HandlerFunctionProvider GetFunctionProvider()
        =>
        new(DataProviders, "HandlerFunction");
}