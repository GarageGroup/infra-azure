using GGroupp;

namespace GarageGroup.Infra;

partial class KeepWarmFunctionBuilder
{
    internal static string BuildFunctionSourceCode(this KeepWarmMetadata metadata)
        =>
        new SourceBuilder(
            metadata.Namespace)
        .AddUsing(
            "GarageGroup.Infra",
            "Microsoft.Azure.Functions.Worker")
        .AppendCodeLine(
            $"public static class {metadata.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"[Function({metadata.FunctionName.AsStringSourceCode()})]",
            $"public static void Run({metadata.BuildTimerTriggerAttributeSourceCode()} TimerInfo timerInfo, FunctionContext context)")
        .BeginLambda()
        .AppendCodeLine(
            $"context?.GetLogger({metadata.FunctionName.AsStringSourceCode()}).LogTimerInfo(timerInfo);")
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static string BuildTimerTriggerAttributeSourceCode(this KeepWarmMetadata metadata)
        =>
        $"[TimerTrigger({metadata.FunctionSchedule.AsStringSourceCode()})]";
}