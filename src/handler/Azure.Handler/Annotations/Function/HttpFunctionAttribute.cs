using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpFunctionAttribute(string name, string method = HttpMethodName.Post) : HandlerFunctionAttribute(name)
{
    public string Method { get; } = method ?? string.Empty;

    public string? Route { get; set; }

    public HttpAuthorizationLevel AuthLevel { get; set; }
}