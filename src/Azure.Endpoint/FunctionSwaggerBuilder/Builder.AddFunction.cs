using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public FunctionSwaggerBuilder AddFunctionEndpoint(EndpointMetadata endpointMetadata)
    {
        if (endpointMetadata is null)
        {
            return this;
        }

        document.Paths ??= new OpenApiPaths();
        var pathItem = GetOrCreatePathItem(document.Paths, endpointMetadata);

        var operationType = ToOperationType(endpointMetadata.Method);

        if (pathItem.Operations is null)
        {
            pathItem.Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [operationType] = endpointMetadata.Operation
            };
        }
        else if (pathItem.Operations.ContainsKey(operationType) is false)
        {
            pathItem.Operations.Add(operationType, endpointMetadata.Operation);
        }

        document.Components ??= new();
        document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var schema in endpointMetadata.Schemas)
        {
            if (document.Components.Schemas.ContainsKey(schema.Key))
            {
                continue;
            }

            document.Components.Schemas.Add(schema.Key, schema.Value);
        }

        return this;
    }

    private static OpenApiPathItem GetOrCreatePathItem(OpenApiPaths paths, EndpointMetadata metadata)
    {
        var path = GetEndpointPath(metadata.Route);
        if (paths.TryGetValue(path, out var pathItem))
        {
            return pathItem;
        }

        var createdItem = new OpenApiPathItem
        {
            Summary = metadata.Summary,
            Description = metadata.Description
        };

        paths.Add(path, createdItem);
        return createdItem;
    }

    private static OperationType ToOperationType(EndpointMethod method)
        =>
        method switch
        {
            EndpointMethod.Get => OperationType.Get,
            EndpointMethod.Post => OperationType.Post,
            EndpointMethod.Put => OperationType.Put,
            EndpointMethod.Delete => OperationType.Delete,
            EndpointMethod.Options => OperationType.Options,
            EndpointMethod.Head => OperationType.Head,
            EndpointMethod.Patch => OperationType.Patch,
            EndpointMethod.Trace => OperationType.Trace,
            _ => OperationType.Post
        };
}