namespace GGroupp.Infra;

internal static partial class FunctionBuilder
{
    private const string EmptyStringConstantSourceCode = "\"\"";

    private static string AsStringSourceCode(this string? source, string defaultSourceCode = "string.Empty")
        =>
        string.IsNullOrEmpty(source) ? defaultSourceCode : $"\"{source.EncodeString()}\"";

    private static string? EncodeString(this string? source)
        =>
        source?.Replace("\"", "\\\"");
}