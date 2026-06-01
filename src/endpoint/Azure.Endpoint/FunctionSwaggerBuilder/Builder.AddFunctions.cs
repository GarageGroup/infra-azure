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
            result = InnerAddFunctionEndpoint(endpointMetadata);
        }

        return result;
    }
}
