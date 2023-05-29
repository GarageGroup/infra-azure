using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeFuncPack;

namespace GarageGroup.Infra.Endpoint;

public static class EndpointSwaggerExtensions
{
    private const string DefaultSwaggerRoute = "swagger/swagger.json";

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
        return request.GetSwaggerUI(swaggerUrl: request.FunctionContext.GetRouteUrl(swaggerRoute));
    }
}