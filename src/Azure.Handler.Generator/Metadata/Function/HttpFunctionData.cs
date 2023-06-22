namespace GarageGroup.Infra;

internal sealed record class HttpFunctionData : BaseFunctionData
{
    public HttpFunctionData(string method, string? functionRoute, int authorizationLevel)
    {
        Method = method ?? string.Empty;
        FunctionRoute = functionRoute;
        AuthorizationLevel = authorizationLevel;
    }

    public string Method { get; }

    public string? FunctionRoute { get; }

    public int AuthorizationLevel { get; }
}