using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class EndpointFunctionSecurityAttribute : Attribute
{
    public EndpointFunctionSecurityAttribute(FunctionAuthorizationLevel level)
        =>
        Level = level;

    public FunctionAuthorizationLevel Level { get; }
}