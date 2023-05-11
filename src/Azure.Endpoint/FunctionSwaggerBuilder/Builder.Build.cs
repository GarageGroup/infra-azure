using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public string Build()
        =>
        document.Serialize(OpenApiSpecVersion.OpenApi3_0, format);
}