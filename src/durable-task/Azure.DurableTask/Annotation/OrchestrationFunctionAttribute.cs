using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class OrchestrationFunctionAttribute(string name) : HandlerFunctionAttribute(name)
{
}