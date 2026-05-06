using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class RefreshableTokenCredentialAttribute(string scheduleExpression) : Attribute
{
    public string ScheduleExpression { get; } = scheduleExpression ?? string.Empty;

    public bool UseMonitor { get; set; }

    public bool RunOnStartup { get; set; }

    public bool IsDisabled { get; set; }
}