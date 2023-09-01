namespace GarageGroup.Infra;

internal sealed record class KeepWarmMetadata
{
    public KeepWarmMetadata(string @namespace, string typeName, string functionName, string functionSchedule)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        FunctionName = functionName ?? string.Empty;
        FunctionSchedule = functionSchedule ?? string.Empty;
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public string FunctionName { get; }

    public string FunctionSchedule { get; }
}