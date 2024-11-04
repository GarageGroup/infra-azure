namespace GarageGroup.Infra;

partial class FunctionBuilder
{
    internal static string BuildFunctionSourceCode()
        =>
        new SourceBuilder(
            "GarageGroup.Infra")
        .AddUsing(
            "System.Threading.Tasks",
            "Microsoft.Azure.Functions.Worker")
        .AppendCodeLine(
            "public static class RefreshableTokenCredentialFunction")
        .BeginCodeBlock()
        .AppendCodeLine(
            "[Function(\"RefreshAzureTokens\")]",
            "[FixedDelayRetry(5, \"00:00:10\")]",
            "public static Task RefreshAzureTokensAsync([TimerTrigger(\"0 */30 * * * *\")] object input, FunctionContext context)")
        .BeginLambda()
        .AppendCodeLine(
            "context.RefreshAzureTokensAsync();")
        .EndLambda()
        .EndCodeBlock()
        .Build();
}
