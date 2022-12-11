using System;
using System.Net;
using System.Net.Mime;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GGroupp.Infra.Endpoint;

public static class EndpointSwaggerExtensions
{
    public static SwaggerOption? GetSwaggerOption(this FunctionContext? context, string prefix = "Swagger")
        =>
        context?.InstanceServices.GetService<IConfiguration>()?.GetSwaggerOptionWithPrefix(prefix);

    public static FunctionSwaggerBuilder CreateBuilder(this SwaggerOption? swaggerOption, string? apiVersion)
        =>
        new(swaggerOption, apiVersion);

    public static HttpResponseData BuildResponseJson(this FunctionSwaggerBuilder builder, HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(request);

        var response = request.CreateResponse(HttpStatusCode.OK);

        var json = builder.BuildJson();
        response.WriteString(json);

        _ = response.Headers.TryAddWithoutValidation("Content-Type", MediaTypeNames.Application.Json);
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

        var url = $"{request.Url.Scheme}://{request.Url.Authority.TrimEnd('/')}/api/swagger/{swaggerOption.ApiVersion}/swagger.json";
        var content = options.GetSwaggerUIContent(url);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(content);

        _ = response.Headers.TryAddWithoutValidation("Content-Type", "text/html;charset=utf-8");
        return response;
    }
}