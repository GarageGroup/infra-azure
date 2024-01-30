using System.Collections.Generic;

namespace GarageGroup.Infra;

public sealed partial class HandlerFunctionProvider(IReadOnlyList<IFunctionDataProvider> functionDataProviders, string typeNameSuffix)
{
    private const string DefaultNamespace = "GarageGroup.Infra";

    private readonly IReadOnlyList<IFunctionDataProvider> functionDataProviders
        =
        functionDataProviders ?? [];

    private readonly string typeNameSuffix
        =
        typeNameSuffix ?? string.Empty;
}