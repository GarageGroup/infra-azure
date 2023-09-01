namespace GarageGroup.Infra;

internal sealed class FunctionSwaggerUIMetadata
{
    internal FunctionSwaggerUIMetadata(string @namespace, string typeName)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
    }

    public string Namespace { get; }

    public string TypeName { get; }
}