using System;
using Microsoft.OpenApi;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public FunctionSwaggerBuilder Configure(Action<OpenApiDocument> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure.Invoke(document);
        return this;
    }
}
