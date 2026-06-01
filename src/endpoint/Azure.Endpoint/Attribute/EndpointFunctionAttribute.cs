using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EndpointFunctionAttribute : Attribute
{
    private const string ObsoleteNameConstructorMessage
        =
        $"{nameof(EndpointFunctionAttribute)}(string name) is obsolete and will be removed in a future version. " +
        $"Use {nameof(EndpointFunctionAttribute)}() and set operation id via EndpointOperationMetadataAttribute.";

    [Obsolete(ObsoleteNameConstructorMessage)]
    public EndpointFunctionAttribute(string name)
        =>
        Name = name ?? string.Empty;

    public EndpointFunctionAttribute()
    {
    }

    public string? Name { get; }

    public bool IsSwaggerHidden { get; init; }
}
