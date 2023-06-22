using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpFunctionAttribute : HandlerFunctionAttribute
{
    public HttpFunctionAttribute(string name, string method = HttpMethodName.Post) : base(name)
        =>
        Method = method ?? string.Empty;

    public string Method { get; }

    public string? Route { get; set; }

    public HttpAuthorizationLevel AuthLevel { get; set; }
}