using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

public static class KeepWarmFunctionExtensions
{
    public static void LogTimerInfo([AllowNull] this ILogger logger, TimerInfo timerInfo)
        =>
        logger?.LogDebug("Last: {last}, Next: {next}", timerInfo?.ScheduleStatus?.Last, timerInfo?.ScheduleStatus?.Next);
}