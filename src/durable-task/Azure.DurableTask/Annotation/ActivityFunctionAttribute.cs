using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ActivityFunctionAttribute : HandlerFunctionAttribute
{
    public ActivityFunctionAttribute(string name) : base(name)
    {
    }
}