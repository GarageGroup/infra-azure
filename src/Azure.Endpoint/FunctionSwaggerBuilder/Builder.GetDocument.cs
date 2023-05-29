using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public ValueTask<OpenApiDocument> GetDocumentAsync(string documentName, CancellationToken cancellationToken = default)
        =>
        ValueTask.FromResult(document);
}