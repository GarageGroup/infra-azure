using System;
using System.Linq;
using GGroupp.Infra;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace GarageGroup.Infra.Endpoint;

public sealed partial class FunctionSwaggerBuilder
{
    private const string BasePathUrl = "/api/";

    private static readonly string[] YamlFormats = new[] { "yaml", "yml" };

    private readonly OpenApiDocument document;

    private readonly OpenApiFormat format;

    public FunctionSwaggerBuilder(SwaggerOption? swaggerOption, string? format)
    {
        document = new()
        {
            Info = swaggerOption.InitializeOpenApiInfo() ?? new()
        };

        this.format = ParseOpenApiFormat(format);
    }

    private static string GetEndpointPath(string route)
        =>
        BasePathUrl + route.TrimStart('/');

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