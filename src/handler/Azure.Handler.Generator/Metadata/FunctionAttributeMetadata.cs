using System;
using System.Collections.Generic;

namespace GarageGroup.Infra;

public sealed record class FunctionAttributeMetadata
{
    public FunctionAttributeMetadata(
        IReadOnlyList<string>? namespaces,
        string typeDisplayName,
        IReadOnlyList<string>? constructorArgumentSourceCodes,
        IReadOnlyList<KeyValuePair<string, string>>? propertySourceCodes)
    {
        Namespaces = namespaces ?? Array.Empty<string>();
        TypeDisplayName = typeDisplayName ?? string.Empty;
        ConstructorArgumentSourceCodes = constructorArgumentSourceCodes ?? Array.Empty<string>();
        PropertySourceCodes = propertySourceCodes ?? Array.Empty<KeyValuePair<string, string>>();
    }

    public IReadOnlyList<string> Namespaces { get; }

    public string TypeDisplayName { get; }

    public IReadOnlyList<string> ConstructorArgumentSourceCodes { get; }

    public IReadOnlyList<KeyValuePair<string, string>> PropertySourceCodes { get; } 
}