using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public abstract class HandlerFunctionAttribute : Attribute
{
    public HandlerFunctionAttribute(string name)
        =>
        Name = name ?? string.Empty;

    public string Name { get; }
}