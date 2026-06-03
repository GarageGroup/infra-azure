using System.Collections.Generic;
using PrimeFuncPack;

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
        bool isAuthorizationRequired,
        bool isSwaggerHidden,
        bool isEndpointSetOperation,
        string? endpointOperationId)
    {
        EndpointType = endpointType;
        ResolverMethodName = resolverMethodName ?? string.Empty;
        FunctionMethodName = functionMethodName ?? string.Empty;
        DependencyFieldName = dependencyFieldName;
        FunctionName = functionName ?? string.Empty;
        ObsoleteData = obsoleteData;
        Arguments = arguments ?? [];
        IsAuthorizationRequired = isAuthorizationRequired;
        IsSwaggerHidden = isSwaggerHidden;
        IsEndpointSetOperation = isEndpointSetOperation;
        EndpointOperationId = endpointOperationId;
    }

    public DisplayedTypeData EndpointType { get; }

    public string ResolverMethodName { get; }

    public string FunctionMethodName { get; }

    public string DependencyFieldName { get; }

    public string FunctionName { get; }

    public ObsoleteData? ObsoleteData { get; }

    public IReadOnlyList<FunctionArgumentMetadata> Arguments { get; }

    public bool IsAuthorizationRequired { get; }

    public bool IsSwaggerHidden { get; }

    public bool IsEndpointSetOperation { get; }

    public string? EndpointOperationId { get; }
}