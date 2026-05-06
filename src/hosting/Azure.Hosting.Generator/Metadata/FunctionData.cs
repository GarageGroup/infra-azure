namespace GarageGroup.Infra;

internal sealed record class FunctionData
{
    public FunctionData(string scheduleExpression, bool useMonitor, bool runOnStartup)
    {
        ScheduleExpression = scheduleExpression ?? string.Empty;
        UseMonitor = useMonitor;
        RunOnStartup = runOnStartup;
    }

    public string ScheduleExpression { get; }

    public bool UseMonitor { get; }

    public bool RunOnStartup { get; }
}