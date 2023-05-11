using System;
using System.Net;
using System.Net.Mime;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using GGroupp.Infra;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace GarageGroup.Infra.Endpoint;

public static class EndpointSwaggerExtensions
{
    public static SwaggerOption? GetSwaggerOption(this FunctionContext? context, string sectionName = "Swagger")
        =>
        context?.InstanceServices.GetService<IConfiguration>()?.GetSwaggerOption(sectionName);

    public static FunctionSwaggerBuilder CreateBuilder(this SwaggerOption? swaggerOption, string? format)
        =>
        new(swaggerOption, format);

    public static HttpResponseData BuildResponse(this FunctionSwaggerBuilder builder, HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(request);

        var response = request.CreateResponse(HttpStatusCode.OK);

        var text = builder.Build();
        response.WriteString(text);

        var contentType = builder.GetFormat() is OpenApiFormat.Yaml ? "application/yaml" : MediaTypeNames.Application.Json;
        _ = response.Headers.TryAddWithoutValidation("Content-Type", contentType);

        return response;
    }

    public static HttpResponseData BuildStandardSwaggerUiResponse(this HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var swaggerOption = request.FunctionContext.GetSwaggerOption("Swagger") ?? new();
        var options = new SwaggerDocOptions
        {
            Title = swaggerOption.ApiName
        };

        var url = $"{request.Url.Scheme}://{request.Url.Authority.TrimEnd('/')}/api/swagger/swagger.json";
        var content = options.GetSwaggerUIContent(url);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(content);

        _ = response.Headers.TryAddWithoutValidation("Content-Type", "text/html;charset=utf-8");
        return response;
    }
}