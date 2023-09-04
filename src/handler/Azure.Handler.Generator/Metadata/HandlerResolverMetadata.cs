namespace GarageGroup.Infra;

public sealed record class HandlerResolverMetadata
{
    public HandlerResolverMetadata(
        DisplayedTypeData handlerType,
        DisplayedTypeData inputType,
        DisplayedTypeData outputType,
        string resolverMethodName,
        string functionMethodName,
        string functionName,
        string? jsonRootPath,
        HandlerFunctionMetadata functionSpecificData)
    {
        HandlerType = handlerType;
        InputType = inputType;
        OutputType = outputType;
        ResolverMethodName = resolverMethodName ?? string.Empty;
        FunctionMethodName = functionMethodName ?? string.Empty;
        FunctionName = functionName ?? string.Empty;
        JsonRootPath = jsonRootPath;
        FunctionSpecificData = functionSpecificData;
    }

    public DisplayedTypeData HandlerType { get; }

    public DisplayedTypeData InputType { get; }

    public DisplayedTypeData OutputType { get; }

    public string ResolverMethodName { get; }

    public string FunctionMethodName { get; }

    public string FunctionName { get; }

    public string? JsonRootPath { get; }

    public HandlerFunctionMetadata FunctionSpecificData { get; }
}