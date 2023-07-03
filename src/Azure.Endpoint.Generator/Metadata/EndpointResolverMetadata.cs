using System;
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
        int authorizationLevel,
        IReadOnlyCollection<string>? httpMethodNames,
        string? httpRoute)
    {
        EndpointType = endpointType;
        ResolverMethodName = resolverMethodName ?? string.Empty;
        FunctionMethodName = functionMethodName ?? string.Empty;
        DependencyFieldName = dependencyFieldName;
        FunctionName = functionName ?? string.Empty;
        AuthorizationLevel = authorizationLevel;
        HttpMethodNames = httpMethodNames ?? Array.Empty<string>();
        HttpRoute = string.IsNullOrEmpty(httpRoute) ? null : httpRoute;
    }

    public DisplayedTypeData EndpointType { get; }

    public string ResolverMethodName { get; }

    public string FunctionMethodName { get; }

    public string DependencyFieldName { get; }

    public string FunctionName { get; }

    public int AuthorizationLevel { get; }

    public IReadOnlyCollection<string> HttpMethodNames { get; }

    public string? HttpRoute { get; }
}