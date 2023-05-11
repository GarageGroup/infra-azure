using System;
using Microsoft.OpenApi.Models;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public FunctionSwaggerBuilder Configure(Action<OpenApiDocument> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        
        configure(document);
        
        return this;
    }
}