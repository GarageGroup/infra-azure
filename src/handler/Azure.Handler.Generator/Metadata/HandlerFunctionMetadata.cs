using System.Collections.Generic;

namespace GarageGroup.Infra;

public sealed record class HandlerFunctionMetadata
{
    public HandlerFunctionMetadata(
        IReadOnlyList<string>? namespaces,
        string responseTypeDisplayName,
        string extensionsMethodName,
        IReadOnlyList<FunctionArgumentMetadata> arguments)
    {
        Namespaces = namespaces ?? [];
        ResponseTypeDisplayName = responseTypeDisplayName ?? string.Empty;
        ExtensionsMethodName = extensionsMethodName ?? string.Empty;
        Arguments = arguments ?? [];
    }

    public IReadOnlyList<string> Namespaces { get; }

    public string ResponseTypeDisplayName { get; }

    public string ExtensionsMethodName { get; }

    public IReadOnlyList<FunctionArgumentMetadata> Arguments { get; }
}