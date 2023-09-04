using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class OrchestrationFunctionAttribute : HandlerFunctionAttribute
{
    public OrchestrationFunctionAttribute(string name) : base(name)
    {
    }
}