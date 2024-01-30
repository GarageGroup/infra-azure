using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace GarageGroup.Infra;

public static partial class SwaggerFunction
{
    private const string DefaultSwaggerUrl = "/api/swagger/swagger.json";

    private const string DefaultSwaggerSection = "Swagger";

    private static readonly string[] YamlFormats = ["yaml", "yml"];

    private static readonly Lazy<string> LazyHtmlTemplate = new(BuildHtmlTemplate);

    private static SwaggerOption? GetSwaggerOption(this FunctionContext? context, string sectionName)
        =>
        context?.InstanceServices.GetService<IConfiguration>()?.GetSwaggerOption(sectionName);

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
        var assembly = Assembly.GetAssembly(typeof(SwashBuckleClient))
            ?? throw new InvalidOperationException($"Assembly for type {typeof(SwashBuckleClient)} was not found");

        var resourceName = $"{typeof(ISwashBuckleClient).Namespace}.EmbededResources.resources.zip";

        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"ManifestResource {resourceName} must be not null");
    }

    private static ZipArchiveEntry GetEntryOrThrow(this ZipArchive archive, string entryName)
        =>
        archive.GetEntry(entryName) ?? throw new InvalidOperationException("Entry '{entryName}' must be not null");

    private static OpenApiFormat ParseOpenApiFormat(string? sourceValue)
    {
        if (string.IsNullOrEmpty(sourceValue))
        {
            return default;
        }

        if (YamlFormats.Contains(sourceValue, StringComparer.InvariantCultureIgnoreCase))
        {
            return OpenApiFormat.Yaml;
        }

        return OpenApiFormat.Json;
    }
}