using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;

namespace GarageGroup.Infra.Endpoint;

internal static class FunctionSwaggerUIHelper
{
    private static readonly Lazy<string> LazyHtmlTemplate = new(BuildHtmlTemplate);

    internal static string GetSwaggerUIContent(this SwaggerDocOptions swaggerOptions, string swaggerUrl)
        =>
        LazyHtmlTemplate.Value
            .Replace("{url}", swaggerUrl)
            .Replace("{title}", swaggerOptions.Title)
            .Replace("{oauth2RedirectUrl}", swaggerOptions.OAuth2RedirectPath)
            .Replace("{clientId}", swaggerOptions.ClientId);

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

    private static string LoadAndUpdateHtml(this string documentHtml, ZipArchive archive, string entryName, string? replacement = null)
    {
        var entry = archive.GetEntryOrThrow(entryName);

        using var stream = entry.Open();
        using var reader = new StreamReader(stream);

        var value = reader.ReadToEnd();
        return string.IsNullOrEmpty(replacement) is false ? documentHtml.Replace(replacement, value) : value;
    }

    private static Stream GetZippedResources()
    {
        var assembly = Assembly.GetAssembly(typeof(SwashBuckleClient));
        if (assembly is null)
        {
            throw new InvalidOperationException($"Assembly for type {typeof(SwashBuckleClient)} was not found");
        }

        var resourceName = $"{typeof(ISwashBuckleClient).Namespace}.EmbededResources.resources.zip";
        var resources = assembly.GetManifestResourceStream(resourceName);

        if (resources is null)
        {
            throw new InvalidOperationException($"ManifestResource {resourceName} must be not null");
        }

        return resources;
    }

    private static ZipArchiveEntry GetEntryOrThrow(this ZipArchive archive, string entryName)
        =>
        archive.GetEntry(entryName) ?? throw new InvalidOperationException("Entry '{entryName}' must be not null");
}