using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class EndpointFunctionSecurityAttribute(FunctionAuthorizationLevel level) : Attribute
{
    public FunctionAuthorizationLevel Level { get; } = level;
}