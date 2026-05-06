namespace GarageGroup.Infra;

internal static partial class FunctionBuilder
{
    private static string GetScheduleExpressionSourceCode(this FunctionData functionData)
        =>
        "\"" + functionData.ScheduleExpression.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}