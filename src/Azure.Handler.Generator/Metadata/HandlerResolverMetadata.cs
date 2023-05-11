using GGroupp;

namespace GarageGroup.Infra;

internal sealed record class HandlerResolverMetadata
{
    public HandlerResolverMetadata(
        DisplayedTypeData handlerType,
        DisplayedTypeData evendDataType,
        string functionAttributeTypeName,
        string functionAttributeNamespace,
        string resolverMethodName,
        string functionMethodName,
        string dependencyFieldName,
        string functionName,
        string? jsonRootPath)
    {
        HandlerType = handlerType;
        EvendDataType = evendDataType;
        FunctionAttributeTypeName = functionAttributeTypeName ?? string.Empty;
        FunctionAttributeNamespace = functionAttributeNamespace ?? string.Empty;
        ResolverMethodName = resolverMethodName ?? string.Empty;
        FunctionMethodName = functionMethodName ?? string.Empty;
        DependencyFieldName = dependencyFieldName;
        FunctionName = functionName ?? string.Empty;
        JsonRootPath = jsonRootPath;
    }

    public DisplayedTypeData HandlerType { get; }

    public DisplayedTypeData EvendDataType { get; }

    public string FunctionAttributeTypeName { get; }

    public string FunctionAttributeNamespace { get; }

    public string ResolverMethodName { get; }

    public string FunctionMethodName { get; }

    public string DependencyFieldName { get; }

    public string FunctionName { get; }

    public string? JsonRootPath { get; }
}