using System.Collections.Generic;

namespace GarageGroup.Infra;

internal sealed record class FunctionProviderMetadata
{
    internal FunctionProviderMetadata(
        string @namespace,
        string typeName,
        DisplayedTypeData providerType,
        IReadOnlyList<EndpointResolverMetadata> resolverTypes)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        ProviderType = providerType;
        ResolverTypes = resolverTypes ?? [];
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public DisplayedTypeData ProviderType { get; }

    public IReadOnlyList<EndpointResolverMetadata> ResolverTypes { get; }
}