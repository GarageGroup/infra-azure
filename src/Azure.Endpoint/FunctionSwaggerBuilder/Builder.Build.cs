using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;

namespace GGroupp.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public string BuildJson()
        =>
        document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);

    public string BuildYaml()
        =>
        document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml);
}