using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeFuncPack;

namespace GarageGroup.Infra.Endpoint;

public static class EndpointSwaggerExtensions
{
    private const string DefaultSwaggerRoute = "swagger/swagger.json";

    private const string FunctionCodeQueryParameterName = "code";

    public static FunctionSwaggerBuilder CreateStandardSwaggerBuilder(this HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var swaggerOption = request.FunctionContext.InstanceServices.GetService<IConfiguration>()?.GetSwaggerOption();
        return new(swaggerOption, request.FunctionContext);
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
        var functionCode = HttpUtility.ParseQueryString(request.Url.Query)[FunctionCodeQueryParameterName];

        if (string.IsNullOrWhiteSpace(functionCode) is false)
        {
            swaggerUrl = swaggerUrl.WithQueryParameter(
                FunctionCodeQueryParameterName,
                functionCode!);
        }

        return request.GetSwaggerUI(swaggerUrl: swaggerUrl);
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
}
