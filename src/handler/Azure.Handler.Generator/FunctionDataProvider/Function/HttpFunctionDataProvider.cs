using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed class HttpFunctionDataProvider : IFunctionDataProvider
{
    public HandlerFunctionMetadata? GetFunctionMetadata(AttributeData functionAttribute, FunctionDataContext _)
    {
        if (functionAttribute.AttributeClass?.IsType("GarageGroup.Infra", "HttpFunctionAttribute") is not true)
        {
            return null;
        }

        return new(
            namespaces: new[]
            {
                "Microsoft.Azure.Functions.Worker.Http",
                "System.Threading",
                "System.Threading.Tasks"
            },
            responseTypeDisplayName: "Task<HttpResponseData>",
            extensionsMethodName: "RunHttpFunctionAsync",
            arguments: new FunctionArgumentMetadata[]
            {
                new(
                    namespaces: default,
                    typeDisplayName: "HttpRequestData",
                    argumentName: "requestData",
                    orderNumber: int.MinValue,
                    extensionMethodArgumentOrder: int.MinValue,
                    resolverMethodArgumentOrder: null,
                    attributes: new[]
                    {
                        BuildHttpTriggerAttributeMetadata(functionAttribute)
                    }),
                new(
                    namespaces: default,
                    typeDisplayName: "CancellationToken",
                    argumentName: "cancellationToken",
                    orderNumber: int.MaxValue,
                    extensionMethodArgumentOrder: int.MaxValue,
                    resolverMethodArgumentOrder: null,
                    attributes: default)
            });
    }

    private static FunctionAttributeMetadata BuildHttpTriggerAttributeMetadata(AttributeData httpAttribute)
    {
        var authorizationLevelSourceCode = GetAuthorizationLevel(httpAttribute) switch
        {
            0 => "AuthorizationLevel.Anonymous",
            1 => "AuthorizationLevel.User",
            2 => "AuthorizationLevel.Function",
            3 => "AuthorizationLevel.System",
            4 => "AuthorizationLevel.Admin",
            var level => "(AuthorizationLevel)" + level
        };

        var method = httpAttribute.GetAttributeValue(1)?.ToString();

        var properties = new Dictionary<string, string>();

        var functionRoute = httpAttribute.GetAttributePropertyValue("Route")?.ToString();
        if (string.IsNullOrEmpty(functionRoute) is false)
        {
            properties["Route"] = functionRoute.AsStringSourceCodeOr();
        }

        return new(
            namespaces: default,
            typeDisplayName: "HttpTrigger",
            constructorArgumentSourceCodes: new[]
            {
                authorizationLevelSourceCode,
                method.AsStringSourceCodeOr()
            },
            propertySourceCodes: properties.ToArray());
    }

    private static int GetAuthorizationLevel(AttributeData httpAttribute)
    {
        var levelValue = httpAttribute.GetAttributePropertyValue("AuthLevel");
        if (levelValue is null)
        {
            return default;
        }

        if (levelValue is not int level)
        {
            throw new InvalidOperationException($"An unexpected bot function authorization level: {levelValue}");
        }

        return level;
    }
}