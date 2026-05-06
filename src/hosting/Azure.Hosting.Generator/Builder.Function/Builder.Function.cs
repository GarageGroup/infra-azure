using System.Text;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class FunctionBuilder
{
    internal static string BuildFunctionSourceCode(FunctionData data)
        =>
        new SourceBuilder(
            "GarageGroup.Infra")
        .AddUsing(
            "System.Threading.Tasks",
            "Microsoft.Azure.Functions.Worker")
        .AppendCodeLines(
            "public static class RefreshableTokenCredentialFunction")
        .BeginCodeBlock()
        .AppendCodeLines(
            "[Function(\"RefreshAzureTokens\")]",
            "[FixedDelayRetry(5, \"00:00:10\")]",
            "public static Task RefreshAzureTokensAsync(")
        .BeginArguments()
        .AppendCodeLines(
            $"[TimerTrigger({data.BuildTimerTriggerAttributeSourceCode()})] object input, FunctionContext context)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLines(
            "context.RefreshAzureTokensAsync();")
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static string BuildTimerTriggerAttributeSourceCode(this FunctionData data)
    {
        var builder = new StringBuilder(data.GetScheduleExpressionSourceCode());

        if (data.UseMonitor)
        {
            builder = builder.Append(", UseMonitor = true");
        }

        if (data.RunOnStartup)
        {
            builder = builder.Append(", RunOnStartup = true");
        }

        return builder.ToString();
    }
}