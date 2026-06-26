using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ActivityFunctionAttribute(string name) : HandlerFunctionAttribute(name);