using System.Collections.Generic;

namespace GarageGroup.Infra;

internal sealed record class EndpointResolverMetadata
{
    public EndpointResolverMetadata(
        DisplayedTypeData endpointType,
        string resolverMethodName,
        string functionMethodName,
        string dependencyFieldName,
        string functionName,
        ObsoleteData? obsoleteData,
        IReadOnlyList<FunctionArgumentMetadata> arguments,
        bool isSwaggerHidden)
    {
        EndpointType = endpointType;
        ResolverMethodName = resolverMethodName ?? string.Empty;
        FunctionMethodName = functionMethodName ?? string.Empty;
        DependencyFieldName = dependencyFieldName;
        FunctionName = functionName ?? string.Empty;
        ObsoleteData = obsoleteData;
        Arguments = arguments ?? [];
        IsSwaggerHidden = isSwaggerHidden;
    }

    public DisplayedTypeData EndpointType { get; }

    public string ResolverMethodName { get; }

    public string FunctionMethodName { get; }

    public string DependencyFieldName { get; }

    public string FunctionName { get; }

    public ObsoleteData? ObsoleteData { get; }

    public IReadOnlyList<FunctionArgumentMetadata> Arguments { get; }

    public bool IsSwaggerHidden { get; }
}