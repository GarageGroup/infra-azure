using System.Collections.Generic;

namespace GarageGroup.Infra;

internal sealed record class FunctionArgumentMetadata
{
    public FunctionArgumentMetadata(
        IReadOnlyList<string>? namespaces,
        string typeDisplayName,
        string argumentName,
        int orderNumber,
        int? extensionMethodArgumentOrder,
        int? resolverMethodArgumentOrder,
        IReadOnlyList<FunctionAttributeMetadata>? attributes)
    {
        Namespaces = namespaces ?? [];
        TypeDisplayName = typeDisplayName ?? string.Empty;
        ArgumentName = argumentName ?? string.Empty;
        OrderNumber = orderNumber;
        ExtensionMethodArgumentOrder = extensionMethodArgumentOrder;
        ResolverMethodArgumentOrder = resolverMethodArgumentOrder;
        Attributes = attributes ?? [];
    }

    public IReadOnlyList<string> Namespaces { get; }

    public string TypeDisplayName { get; }

    public string ArgumentName { get; }

    public int OrderNumber { get; }

    public int? ExtensionMethodArgumentOrder { get; }

    public int? ResolverMethodArgumentOrder { get; }

    public IReadOnlyList<FunctionAttributeMetadata> Attributes { get; }
}