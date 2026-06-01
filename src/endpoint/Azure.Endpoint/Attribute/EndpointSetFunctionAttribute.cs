using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EndpointSetFunctionAttribute : Attribute
{
    public bool IsSwaggerHidden { get; init; }
}