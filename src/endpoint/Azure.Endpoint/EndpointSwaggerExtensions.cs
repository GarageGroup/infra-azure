using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeFuncPack;

namespace GarageGroup.Infra.Endpoint;

public static class EndpointSwaggerExtensions
{
    private const string DefaultSwaggerRoute = "swagger/swagger.json";

    private const string FunctionCodeQueryParameterName = "code";

    private const string HideFunctionCodeAuthorizationQueryParameterName = "hideFunctionCodeAuthorization";

    public static FunctionSwaggerBuilder CreateStandardSwaggerBuilder(this HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var swaggerOption = request.FunctionContext.InstanceServices.GetService<IConfiguration>()?.GetSwaggerOption();
        var hideFunctionCodeAuthorization = request.GetHideFunctionCodeAuthorization();

        return new(swaggerOption, request.FunctionContext, hideFunctionCodeAuthorization);
    }

    public static Task<HttpResponseData> BuildResponseAsync(
        this FunctionSwaggerBuilder builder, HttpRequestData request, string? format, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(request);

        return Dependency.Of(builder).GetSwaggerDocumentAsync(request, format, cancellationToken);
    }

    public static HttpResponseData BuildStandardSwaggerUiResponse(
        this HttpRequestData request, string? swaggerRoute = DefaultSwaggerRoute)
    {
        ArgumentNullException.ThrowIfNull(request);

        var swaggerUrl = request.FunctionContext.GetRouteUrl(swaggerRoute);
        var queryParameters = HttpUtility.ParseQueryString(request.Url.Query);

        swaggerUrl = swaggerUrl
            .WithQueryParameterIfSpecified(queryParameters, FunctionCodeQueryParameterName)
            .WithQueryParameterIfSpecified(queryParameters, HideFunctionCodeAuthorizationQueryParameterName);

        var swaggerOption = request.FunctionContext.InstanceServices.GetService<IConfiguration>()?.GetSwaggerOption();
        var swaggerOptions = new SwaggerDocOptions
        {
            Title = swaggerOption?.ApiName ?? string.Empty
        };

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(swaggerOptions.GetSwaggerUIContent(swaggerUrl));

        _ = response.Headers.TryAddWithoutValidation("Content-Type", "text/html;charset=utf-8");
        return response;
    }

    private static string WithQueryParameter(this string url, string parameterName, string parameterValue)
    {
        var queryStartIndex = url.IndexOf('?', StringComparison.Ordinal);
        if (queryStartIndex < 0)
        {
            return $"{url}?{parameterName}={Uri.EscapeDataString(parameterValue)}";
        }

        var query = url[(queryStartIndex + 1)..];
        var queryParameters = HttpUtility.ParseQueryString(query);
        queryParameters[parameterName] = parameterValue;

        var path = url[..queryStartIndex];
        return $"{path}?{queryParameters}";
    }

    private static string WithQueryParameterIfSpecified(
        this string url, NameValueCollection queryParameters, string parameterName)
    {
        var parameterValue = queryParameters[parameterName];
        if (string.IsNullOrWhiteSpace(parameterValue))
        {
            return url;
        }

        return url.WithQueryParameter(parameterName, parameterValue!);
    }

    private static bool GetHideFunctionCodeAuthorization(this HttpRequestData request)
    {
        var queryParameters = HttpUtility.ParseQueryString(request.Url.Query);
        var parameterValue = queryParameters[HideFunctionCodeAuthorizationQueryParameterName];

        return bool.TryParse(parameterValue, out var result) && result;
    }
}