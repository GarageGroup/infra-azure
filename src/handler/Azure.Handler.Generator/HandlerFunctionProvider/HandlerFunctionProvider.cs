using System;
using System.Collections.Generic;

namespace GarageGroup.Infra;

public sealed partial class HandlerFunctionProvider
{
    private const string DefaultNamespace = "GarageGroup.Infra";

    private readonly IReadOnlyList<IFunctionDataProvider> functionDataProviders;

    private readonly string typeNameSuffix;

    public HandlerFunctionProvider(IReadOnlyList<IFunctionDataProvider> functionDataProviders, string typeNameSuffix)
    {
        this.functionDataProviders = functionDataProviders ?? Array.Empty<IFunctionDataProvider>();
        this.typeNameSuffix = typeNameSuffix ?? string.Empty;
    }
}