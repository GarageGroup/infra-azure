namespace GarageGroup.Infra;

internal sealed record class HealthCheckMetadata
{
    public HealthCheckMetadata(string @namespace, string typeName, string functionName, string? functionRoute, int authorizationLevel)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        FunctionName = functionName ?? string.Empty;
        FunctionRoute = functionRoute ?? string.Empty;
        AuthorizationLevel = authorizationLevel;
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public string FunctionName { get; }

    public string? FunctionRoute { get; }

    public int AuthorizationLevel { get; }
}