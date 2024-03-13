using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EventGridFunctionAttribute(string name) : HandlerFunctionAttribute(name)
{
}