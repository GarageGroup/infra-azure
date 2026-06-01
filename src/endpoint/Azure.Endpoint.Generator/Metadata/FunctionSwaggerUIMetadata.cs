namespace GarageGroup.Infra;

internal sealed class FunctionSwaggerUIMetadata
{
    internal FunctionSwaggerUIMetadata(string @namespace, string typeName, int authorizationLevel)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        AuthorizationLevel = authorizationLevel;
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public int AuthorizationLevel { get; }
}
