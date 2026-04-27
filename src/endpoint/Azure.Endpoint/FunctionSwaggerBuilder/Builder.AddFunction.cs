using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OpenApi;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerBuilder
{
    public FunctionSwaggerBuilder AddFunctionEndpoint(EndpointMetadata endpointMetadata)
    {
        if (endpointMetadata is null)
        {
            return this;
        }

        document.Paths ??= [];
        var pathItem = GetOrCreatePathItem(document.Paths, endpointMetadata);

        var operationType = ToOperationType(endpointMetadata.Method);
        var operations = GetOrCreateOperations(pathItem);

        if (operations.ContainsKey(operationType) is false)
        {
            operations.Add(operationType, endpointMetadata.Operation);
        }

        document.Components ??= new();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>(StringComparer.InvariantCultureIgnoreCase);

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

    private IOpenApiPathItem GetOrCreatePathItem(OpenApiPaths paths, EndpointMetadata metadata)
    {
        var path = context.GetRouteUrl(metadata.Route);
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

    private static Dictionary<HttpMethod, OpenApiOperation> GetOrCreateOperations(IOpenApiPathItem pathItem)
    {
        if (pathItem.Operations is not null)
        {
            return pathItem.Operations;
        }

        if (pathItem is not OpenApiPathItem concretePathItem)
        {
            throw new InvalidOperationException($"Path item must be of type {typeof(OpenApiPathItem)} when operations are not initialized");
        }

        return concretePathItem.Operations = new Dictionary<HttpMethod, OpenApiOperation>();
    }

    private static HttpMethod ToOperationType(EndpointMethod method)
        =>
        method switch
        {
            EndpointMethod.Get => HttpMethod.Get,
            EndpointMethod.Post => HttpMethod.Post,
            EndpointMethod.Put => HttpMethod.Put,
            EndpointMethod.Delete => HttpMethod.Delete,
            EndpointMethod.Options => HttpMethod.Options,
            EndpointMethod.Head => HttpMethod.Head,
            EndpointMethod.Patch => HttpMethod.Patch,
            EndpointMethod.Trace => HttpMethod.Trace,
            _ => HttpMethod.Post
        };
}
