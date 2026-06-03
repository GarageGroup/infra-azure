using System.Collections.Generic;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public FunctionSwaggerBuilder AddFunctionEndpoints(IReadOnlyCollection<EndpointMetadata> metadata)
    {
        if (metadata?.Count is not > 0)
        {
            return this;
        }

        var result = this;
        foreach (var endpointMetadata in metadata)
        {
            result = InnerAddFunctionEndpoint(endpointMetadata, false);
        }

        return result;
    }

    public FunctionSwaggerBuilder AddFunctionEndpoints(
        IReadOnlyCollection<EndpointMetadata> metadata, bool isAuthorizationRequired)
    {
        if (metadata?.Count is not > 0)
        {
            return this;
        }

        var result = this;
        foreach (var endpointMetadata in metadata)
        {
            result = InnerAddFunctionEndpoint(endpointMetadata, isAuthorizationRequired);
        }

        return result;
    }
}