using System;
using Microsoft.Azure.Functions.Worker;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Class)]
public sealed class HealthCheckFuncAttribute : Attribute
{
    public HealthCheckFuncAttribute(string name, string route = "health")
    {
        Name = name ?? string.Empty;
        Route = route ?? string.Empty;
    }

    public string Name { get; }

    public string Route { get; }

    public AuthorizationLevel AuthLevel { get; set; }
}