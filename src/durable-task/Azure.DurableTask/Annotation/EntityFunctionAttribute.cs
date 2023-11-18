using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EntityFunctionAttribute : HandlerFunctionAttribute
{
    public EntityFunctionAttribute(string name) : base(name)
    {
    }

    public string? EntityName { get; set; }
}