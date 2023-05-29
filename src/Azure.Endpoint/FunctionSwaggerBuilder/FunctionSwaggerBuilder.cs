using Microsoft.Azure.Functions.Worker;
using Microsoft.OpenApi.Models;

namespace GarageGroup.Infra.Endpoint;

public sealed partial class FunctionSwaggerBuilder : ISwaggerDocumentProvider
{
    private readonly OpenApiDocument document;

    private readonly FunctionContext context;

    public FunctionSwaggerBuilder(SwaggerOption? swaggerOption, FunctionContext context)
    {
        document = new()
        {
            Info = swaggerOption.InitializeOpenApiInfo() ?? new()
        };

        this.context = context;
    }
}