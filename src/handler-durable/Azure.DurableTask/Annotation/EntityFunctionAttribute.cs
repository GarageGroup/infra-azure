using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EntityFunctionAttribute(string name) : HandlerFunctionAttribute(name)
{
    public string? EntityName { get; set; }
}