using System.Collections.Generic;

namespace GarageGroup.Infra;

internal sealed record class FunctionAttributeMetadata
{
    public FunctionAttributeMetadata(
        IReadOnlyList<string>? namespaces,
        string typeDisplayName,
        IReadOnlyList<string>? constructorArgumentSourceCodes,
        IReadOnlyList<KeyValuePair<string, string>>? propertySourceCodes)
    {
        Namespaces = namespaces ?? [];
        TypeDisplayName = typeDisplayName ?? string.Empty;
        ConstructorArgumentSourceCodes = constructorArgumentSourceCodes ?? [];
        PropertySourceCodes = propertySourceCodes ?? [];
    }

    public IReadOnlyList<string> Namespaces { get; }

    public string TypeDisplayName { get; }

    public IReadOnlyList<string> ConstructorArgumentSourceCodes { get; }

    public IReadOnlyList<KeyValuePair<string, string>> PropertySourceCodes { get; } 
}