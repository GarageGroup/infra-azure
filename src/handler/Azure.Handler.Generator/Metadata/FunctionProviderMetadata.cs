using System.Collections.Generic;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public sealed record class FunctionProviderMetadata
{
    internal FunctionProviderMetadata(
        string @namespace,
        string typeName,
        DisplayedTypeData providerType,
        IReadOnlyList<HandlerResolverMetadata> resolverTypes)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        ProviderType = providerType;
        ResolverTypes = resolverTypes ?? [];
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public DisplayedTypeData ProviderType { get; }

    public IReadOnlyList<HandlerResolverMetadata> ResolverTypes { get; }
}
