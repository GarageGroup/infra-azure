using System;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi;
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

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(request.FunctionContext.CancellationToken, cancellationToken);
        var documentProvider = documentProviderDependency.Resolve(request.FunctionContext.InstanceServices);

        var document = await documentProvider.GetDocumentAsync(string.Empty, tokenSource.Token).ConfigureAwait(false);
        var isYamlFormat = IsOpenApiYamlFormat(format);

        var openApiFormat = isYamlFormat ? OpenApiConstants.Yaml : OpenApiConstants.Json;
        var text = await document.SerializeAsync(OpenApiSpecVersion.OpenApi3_0, openApiFormat, cancellationToken).ConfigureAwait(false);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(text);

        var contentType = isYamlFormat ? MediaTypeNames.Application.Yaml : MediaTypeNames.Application.Json;
        _ = response.Headers.TryAddWithoutValidation("Content-Type", contentType);

        return response;
    }
}