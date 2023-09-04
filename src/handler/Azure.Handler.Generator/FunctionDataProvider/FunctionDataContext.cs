namespace GarageGroup.Infra;

public sealed record class FunctionDataContext
{
    public FunctionDataContext(
        DisplayedTypeData handlerType,
        DisplayedTypeData inputType,
        DisplayedTypeData outputType)
    {
        HandlerType = handlerType;
        InputType = inputType;
        OutputType = outputType;
    }

    public DisplayedTypeData HandlerType { get; }

    public DisplayedTypeData InputType { get; }

    public DisplayedTypeData OutputType { get; }
}