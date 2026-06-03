using Microsoft.Azure.Functions.Worker;
using Microsoft.OpenApi;

namespace GarageGroup.Infra.Endpoint;

public sealed partial class FunctionSwaggerBuilder : ISwaggerDocumentProvider
{
    private const string FunctionKeyHeaderName = "x-functions-key";

    private const string FunctionKeySecuritySchemeName = "FunctionKey";

    private readonly OpenApiDocument document;

    private readonly FunctionContext context;

    private readonly bool hideFunctionCodeAuthorization;

    public FunctionSwaggerBuilder(SwaggerOption? swaggerOption, FunctionContext context, bool hideFunctionCodeAuthorization = false)
    {
        document = new()
        {
            Info = swaggerOption.InitializeOpenApiInfo() ?? new()
        };

        this.context = context;
        this.hideFunctionCodeAuthorization = hideFunctionCodeAuthorization;
    }
}