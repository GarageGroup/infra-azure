using System;

namespace GGroupp.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EndpointFunctionAttribute : Attribute
{
    public EndpointFunctionAttribute(string name)
        =>
        Name = name ?? string.Empty;

    public string Name { get; }
}