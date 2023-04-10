using System;
using Microsoft.OpenApi.Models;

namespace GGroupp.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public FunctionSwaggerBuilder Configure(Action<OpenApiDocument> configure)
    {
        configure(document);
        return this;
    }
}