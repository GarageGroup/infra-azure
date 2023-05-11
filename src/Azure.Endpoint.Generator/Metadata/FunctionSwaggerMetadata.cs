namespace GarageGroup.Infra;

internal sealed class FunctionSwaggerMetadata
{
    internal FunctionSwaggerMetadata(string @namespace, string typeName)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
    }

    public string Namespace { get; }

    public string TypeName { get; }
}