using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class EndpointFunctionSwaggerUIAttribute(FunctionAuthorizationLevel level = FunctionAuthorizationLevel.Function) : Attribute
{
    public FunctionAuthorizationLevel Level { get; } = level;
}