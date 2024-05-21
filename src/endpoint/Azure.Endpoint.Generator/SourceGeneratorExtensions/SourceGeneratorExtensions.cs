using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string DefaultNamespace = "GarageGroup.Infra";

    private const string EndpointNamespace = "GarageGroup.Infra.Endpoint";

    private const string ResolverStandardStart = "Use";

    private const string ResolverStandardEnd = "Endpoint";

    private static IReadOnlyList<FunctionArgumentMetadata> BuildDefaultArguments(int authorizationLevel, AttributeData? endpointAttribute)
    {
        return
        [
            new(
                namespaces: default,
                typeDisplayName: "HttpRequestData",
                argumentName: "requestData",
                orderNumber: int.MinValue,
                extensionMethodArgumentOrder: int.MinValue,
                resolverMethodArgumentOrder: null,
                attributes:
                [
                    BuildHttpTriggerAttributeMetadata(authorizationLevel, endpointAttribute)
                ]),
            new(
                namespaces: default,
                typeDisplayName: "CancellationToken",
                argumentName: "cancellationToken",
                orderNumber: int.MaxValue,
                extensionMethodArgumentOrder: int.MaxValue,
                resolverMethodArgumentOrder: null,
                attributes: default)
        ];
    }

    private static FunctionAttributeMetadata BuildHttpTriggerAttributeMetadata(int authorizationLevel, AttributeData? endpointAttribute)
    {
        var authorizationLevelSourceCode = authorizationLevel switch
        {
            0 => "AuthorizationLevel.Anonymous",
            1 => "AuthorizationLevel.User",
            2 => "AuthorizationLevel.Function",
            3 => "AuthorizationLevel.System",
            4 => "AuthorizationLevel.Admin",
            _ => "(AuthorizationLevel)" + authorizationLevel
        };

        var methodBuilder = new StringBuilder();
        foreach (var method in endpointAttribute.GetHttpMethodNames())
        {
            if (string.IsNullOrEmpty(method))
            {
                continue;
            }

            if (methodBuilder.Length > 0)
            {
                methodBuilder = methodBuilder.Append(", ");
            }

            methodBuilder = methodBuilder.Append(method.AsStringSourceCodeOr());
        }

        var properties = new Dictionary<string, string>();
        var route = endpointAttribute.GetHttpRoute();

        if (string.IsNullOrEmpty(route) is false)
        {
            properties["Route"] = route.AsStringSourceCodeOr();
        }

        return new(
            namespaces: default,
            typeDisplayName: "HttpTrigger",
            constructorArgumentSourceCodes:
            [
                authorizationLevelSourceCode,
                methodBuilder.ToString()
            ],
            propertySourceCodes: [.. properties]);
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

    private static string RemoveStandardStart(this string name)
    {
        var startLength = ResolverStandardStart.Length;
        if (name.Length <= startLength)
        {
            return name;
        }

        if (name.StartsWith(ResolverStandardStart, StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return name;
        }

        return name.Substring(startLength);
    }

    private static string RemoveStandardEnd(this string name)
    {
        var endLength = ResolverStandardEnd.Length;
        if (name.Length <= endLength)
        {
            return name;
        }

        if (name.EndsWith(ResolverStandardEnd, StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return name;
        }

        return name.Substring(0, name.Length - endLength);
    }

    private static string SetLastWordAsFirst(this string name)
    {
        var lastCapital =  Array.FindLastIndex(name.ToCharArray(), char.IsUpper);
        if (lastCapital < 0)
        {
            return name;
        }

        return name.Substring(lastCapital) + name.Substring(0, lastCapital);
    }

    private static string FromLowerCase(this string name)
    {
        if (name.Length <= 1)
        {
            return name.ToLowerInvariant();
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item is null)
            {
                continue;
            }

            yield return item;
        }
    }

    private static InvalidOperationException CreateInvalidMethodException(this IMethodSymbol resolverMethod, string message)
        =>
        new($"Function resolver method {resolverMethod.ContainingType?.Name}.{resolverMethod.Name} {message}");
}