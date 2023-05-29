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

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(text);

        var contentType = openApiFormat is OpenApiFormat.Yaml ? "application/yaml" : MediaTypeNames.Application.Json;
        _ = response.Headers.TryAddWithoutValidation("Content-Type", contentType);

        return response;
    }
}