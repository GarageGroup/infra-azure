namespace GGroupp.Infra;

internal static partial class KeepWarmFunctionBuilder
{
    private static string AsStringSourceCode(this string? source, string defaultSourceCode = "\"\"")
        =>
        string.IsNullOrEmpty(source) ? defaultSourceCode : $"\"{source.EncodeString()}\"";

    private static string? EncodeString(this string? source)
        =>
        source?.Replace("\"", "\\\"");
}