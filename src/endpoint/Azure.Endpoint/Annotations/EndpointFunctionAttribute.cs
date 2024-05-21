using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EndpointFunctionAttribute(string name) : Attribute
{
    public string Name { get; } = name ?? string.Empty;

    public bool IsSwaggerHidden { get; init; }
}