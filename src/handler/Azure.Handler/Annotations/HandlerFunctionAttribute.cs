using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class HandlerFunctionAttribute(string name) : Attribute
{
    public string Name { get; } = name ?? string.Empty;
}