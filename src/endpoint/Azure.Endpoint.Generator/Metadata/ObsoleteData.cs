namespace GarageGroup.Infra;

internal sealed record class ObsoleteData
{
    public ObsoleteData(string? message, bool? isError, string? diagnosticId, string? urlFormat)
    {
        Message = message;
        IsError = isError;
        DiagnosticId = diagnosticId;
        UrlFormat = urlFormat;
    }

    public string? Message { get; }

    public bool? IsError { get; }

    public string? DiagnosticId { get; }

    public string? UrlFormat { get; }
}