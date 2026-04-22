using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpFunctionAttribute(string name, string method = HttpMethodName.Post) : HandlerFunctionAttribute(name)
{
    public string Method { get; } = method ?? string.Empty;

    public string? Route { get; set; }

    public HttpAuthorizationLevel AuthLevel { get; set; }

    public string? ReadInputFunc { get; set; }

    public Type? ReadInputFuncType { get; set; }

    public string? CreateSuccessResponseFunc { get; set; }

    public Type? CreateSuccessResponseFuncType { get; set; }

    public string? CreateFailureResponseFunc { get; set; }

    public Type? CreateFailureResponseFuncType { get; set; }
}
