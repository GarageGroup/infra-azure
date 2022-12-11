using Microsoft.OpenApi.Models;

namespace GGroupp.Infra.Endpoint;

public sealed partial class FunctionSwaggerBuilder
{
    private const string BasePathUrl = "/api/";

    private readonly OpenApiDocument document;

    public FunctionSwaggerBuilder(SwaggerOption? swaggerOption, string? apiVersion)
        =>
        document = new()
        {
            Info = CreateInfo(swaggerOption, apiVersion)
        };

    private static OpenApiInfo? CreateInfo(SwaggerOption? swaggerOption, string? apiVersion)
    {
        var info = swaggerOption.InitializeOpenApiInfo();
        if (string.IsNullOrEmpty(apiVersion))
        {
            return info;
        }

        if (info is null)
        {
            return new()
            {
                Version = apiVersion
            };
        }

        info.Version = apiVersion;
        return info;
    }

    private static string GetEndpointPath(string route)
        =>
        BasePathUrl + route.TrimStart('/');
}