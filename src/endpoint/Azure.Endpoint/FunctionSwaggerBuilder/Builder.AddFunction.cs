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

        return InnerAddFunctionEndpoint(endpointMetadata, false);
    }

    public FunctionSwaggerBuilder AddFunctionEndpoint(EndpointMetadata endpointMetadata, bool isAuthorizationRequired)
    {
        if (endpointMetadata is null)
        {
            return this;
        }

        return InnerAddFunctionEndpoint(endpointMetadata, isAuthorizationRequired);
    }

    private FunctionSwaggerBuilder InnerAddFunctionEndpoint(EndpointMetadata endpointMetadata, bool isAuthorizationRequired)
    {
        document.Paths ??= [];
        var pathItem = GetOrCreatePathItem(document.Paths, endpointMetadata);

        var operationType = ToOperationType(endpointMetadata.Method);
        var operations = GetOrCreateOperations(pathItem);

        if (operations.ContainsKey(operationType) is false)
        {
            var operation = new OpenApiOperation(endpointMetadata.Operation);
            AddFunctionKeySecurityIfNecessary(operation, document, isAuthorizationRequired);

            operations.Add(operationType, operation);
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

        return concretePathItem.Operations = [];
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

    private void AddFunctionKeySecurityIfNecessary(
        OpenApiOperation operation, OpenApiDocument document, bool isAuthorizationRequired)
    {
        if (isAuthorizationRequired is false || hideFunctionCodeAuthorization)
        {
            return;
        }

        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.InvariantCultureIgnoreCase);

        if (document.Components.SecuritySchemes.ContainsKey(FunctionKeySecuritySchemeName) is false)
        {
            document.Components.SecuritySchemes[FunctionKeySecuritySchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = FunctionKeyHeaderName,
                In = ParameterLocation.Header
            };
        }

        operation.Security ??= [];
        operation.Security.Add(
            item: new()
            {
                [new(FunctionKeySecuritySchemeName, document)] = []
            });
    }
}