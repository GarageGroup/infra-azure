using System;
using System.Net;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.Functions.Worker.Http;

namespace GarageGroup.Infra;

partial class SwaggerFunction
{
    public static HttpResponseData GetSwaggerUI(
        this HttpRequestData request,
        string swaggerSection = DefaultSwaggerSection,
        string swaggerUrl = DefaultSwaggerUrl)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.InnerBuildSwaggerUiResponse(request.FunctionContext.GetSwaggerOption(DefaultSwaggerSection) ?? new(), swaggerUrl);
    }

    public static HttpResponseData GetSwaggerUI(
        this HttpRequestData request,
        SwaggerOption swaggerOption,
        string swaggerUrl = DefaultSwaggerUrl)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.InnerBuildSwaggerUiResponse(swaggerOption ?? new(), swaggerUrl);
    }

    private static HttpResponseData InnerBuildSwaggerUiResponse(
        this HttpRequestData request,
        SwaggerOption swaggerOption,
        string swaggerUrl)
    {
        var options = new SwaggerDocOptions
        {
            Title = swaggerOption.ApiName
        };

        var content = options.GetSwaggerUIContent(swaggerUrl);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(content);

        _ = response.Headers.TryAddWithoutValidation("Content-Type", "text/html;charset=utf-8");
        return response;
    }

    private static string GetSwaggerUIContent(this SwaggerDocOptions swaggerOptions, string swaggerUrl)
        =>
        LazyHtmlTemplate.Value
            .Replace("{url}", swaggerUrl)
            .Replace("{title}", swaggerOptions.Title)
            .Replace("{oauth2RedirectUrl}", swaggerOptions.OAuth2RedirectPath)
            .Replace("{clientId}", swaggerOptions.ClientId);
}