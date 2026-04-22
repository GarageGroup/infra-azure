using PrimeFuncPack;

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
        string? readInputFuncName,
        DisplayedTypeData? readInputFuncType,
        string? createSuccessResponseFuncName,
        DisplayedTypeData? createSuccessResponseFuncType,
        string? createFailureResponseFuncName,
        DisplayedTypeData? createFailureResponseFuncType,
        string? jsonRootPath,
        HandlerFunctionMetadata functionSpecificData)
    {
        HandlerType = handlerType;
        InputType = inputType;
        OutputType = outputType;
        ResolverMethodName = resolverMethodName ?? string.Empty;
        FunctionMethodName = functionMethodName ?? string.Empty;
        FunctionName = functionName ?? string.Empty;
        ReadInputFuncName = readInputFuncName;
        ReadInputFuncType = readInputFuncType;
        CreateSuccessResponseFuncName = createSuccessResponseFuncName;
        CreateSuccessResponseFuncType = createSuccessResponseFuncType;
        CreateFailureResponseFuncName = createFailureResponseFuncName;
        CreateFailureResponseFuncType = createFailureResponseFuncType;
        JsonRootPath = jsonRootPath;
        FunctionSpecificData = functionSpecificData;
    }

    public DisplayedTypeData HandlerType { get; }

    public DisplayedTypeData InputType { get; }

    public DisplayedTypeData OutputType { get; }

    public string ResolverMethodName { get; }

    public string FunctionMethodName { get; }

    public string FunctionName { get; }

    public string? ReadInputFuncName { get; }

    public DisplayedTypeData? ReadInputFuncType { get; }

    public string? CreateSuccessResponseFuncName { get; }

    public DisplayedTypeData? CreateSuccessResponseFuncType { get; }

    public string? CreateFailureResponseFuncName { get; }

    public DisplayedTypeData? CreateFailureResponseFuncType { get; }

    public string? JsonRootPath { get; }

    public HandlerFunctionMetadata FunctionSpecificData { get; }
}
