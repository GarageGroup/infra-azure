using System;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace GarageGroup.Infra.Endpoint;

internal static partial class FunctionSwaggerUIHelper
{
    private const string FunctionCodeQueryParameterName = "code";

    private const string FunctionKeySecuritySchemeName = "FunctionKey";

    private static readonly Regex WindowUiAssignmentRegex
        =
        BuildWindowUiAssignmentRegex();

    private static readonly Lazy<string> LazyHtmlTemplate
        =
        new(BuildHtmlTemplate);

    [GeneratedRegex(@"(?m)^(?<indent>\s*)window\.ui\s*=\s*ui;?\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex BuildWindowUiAssignmentRegex();

    private static string BuildHtmlTemplate()
    {
        using var stream = GetZippedResources();
        using var archive = new ZipArchive(stream);

        return string.Empty
            .LoadAndUpdateHtml(archive, "index.html")
            .LoadAndUpdateHtml(archive, "swagger-ui.css", "{style}")
            .LoadAndUpdateHtml(archive, "swagger-ui-bundle.js", "{bundle.js}")
            .LoadAndUpdateHtml(archive, "swagger-ui-standalone-preset.js", "{standalone-preset.js}");
    }
}