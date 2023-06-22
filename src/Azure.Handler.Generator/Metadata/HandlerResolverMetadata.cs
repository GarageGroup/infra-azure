using GGroupp;

namespace GarageGroup.Infra;

internal sealed record class HandlerResolverMetadata
{
    public HandlerResolverMetadata(
        DisplayedTypeData handlerType,
        DisplayedTypeData inputType,
        DisplayedTypeData outputType,
        string resolverMethodName,
        string functionMethodName,
        string functionName,
        string? jsonRootPath,
        BaseFunctionData functionData)
    {
        HandlerType = handlerType;
        InputType = inputType;
        OutputType = outputType;
        ResolverMethodName = resolverMethodName ?? string.Empty;
        FunctionMethodName = functionMethodName ?? string.Empty;
        FunctionName = functionName ?? string.Empty;
        JsonRootPath = jsonRootPath;
        FunctionData = functionData;
    }

    public DisplayedTypeData HandlerType { get; }

    public DisplayedTypeData InputType { get; }

    public DisplayedTypeData OutputType { get; }

    public string ResolverMethodName { get; }

    public string FunctionMethodName { get; }

    public string FunctionName { get; }

    public string? JsonRootPath { get; }

    public BaseFunctionData FunctionData { get; }
}