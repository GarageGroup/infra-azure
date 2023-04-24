using System;

namespace GGroupp.Infra;

[AttributeUsage(AttributeTargets.Class)]
public sealed class KeepWarmFunctionAttribute : Attribute
{
    public KeepWarmFunctionAttribute(string name, string schedule = "0 */5 * * * *")
    {
        Name = name ?? string.Empty;
        Schedule = schedule ?? string.Empty;
    }

    public string Name { get; }
    
    public string Schedule { get; }
}