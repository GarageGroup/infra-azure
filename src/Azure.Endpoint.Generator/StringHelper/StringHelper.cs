namespace GarageGroup.Infra;

internal static partial class StringHelper
{
    internal const string EmptyStringConstantSourceCode = "\"\"";

    internal static string AsStringSourceCode(this string? source, string defaultSourceCode = "string.Empty")
        =>
        string.IsNullOrEmpty(source) ? defaultSourceCode : $"\"{source.EncodeString()}\"";

    internal static string? EncodeString(this string? source)
        =>
        source?.Replace("\"", "\\\"");
}