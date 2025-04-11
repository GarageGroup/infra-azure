using System;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class SwaggerFunction
{
    public static async Task<HttpResponseData> GetSwaggerDocumentAsync<TSwaggerDocumentProvider>(
        this Dependency<TSwaggerDocumentProvider> documentProviderDependency,
        HttpRequestData request,
        string? format,
        CancellationToken cancellationToken)
        where TSwaggerDocumentProvider : ISwaggerDocumentProvider
    {
        ArgumentNullException.ThrowIfNull(documentProviderDependency);
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(request.FunctionContext.CancellationToken, cancellationToken);
        var documentProvider = documentProviderDependency.Resolve(request.FunctionContext.InstanceServices);

        var document = await documentProvider.GetDocumentAsync(string.Empty, tokenSource.Token).ConfigureAwait(false);
        var openApiFormat = ParseOpenApiFormat(format);

        var text = document.Serialize(OpenApiSpecVersion.OpenApi3_0, openApiFormat);

        // temporary fix: swagerUI hasn't supported OpenAPI v3.0.4 yet
        if (openApiFormat is OpenApiFormat.Json)
        {
            text = text.Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.1\"");
        }
        else if (openApiFormat is OpenApiFormat.Yaml)
        {
            text = text.Replace("openapi: 3.0.4", "openapi: 3.0.1");
        }

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(text);

        var contentType = openApiFormat is OpenApiFormat.Yaml ? "application/yaml" : MediaTypeNames.Application.Json;
        _ = response.Headers.TryAddWithoutValidation("Content-Type", contentType);

        return response;
    }
}