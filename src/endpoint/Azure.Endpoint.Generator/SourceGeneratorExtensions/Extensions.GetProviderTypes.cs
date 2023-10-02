using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    internal static IReadOnlyCollection<FunctionProviderMetadata> GetFunctionProviderTypes(this GeneratorExecutionContext context)
    {
        var visitor = new ExportedTypesCollector(context.CancellationToken);
        visitor.VisitNamespace(context.Compilation.GlobalNamespace);

        return visitor.GetNonPrivateTypes().Select(GetFunctionMetadata).NotNull().ToArray();
    }

    private static FunctionProviderMetadata? GetFunctionMetadata(INamedTypeSymbol typeSymbol)
    {
        var providerAttribute = typeSymbol.GetAttributes().FirstOrDefault(IsFunctionProviderAttribute);
        if (providerAttribute is null)
        {
            return null;
        }

        if (typeSymbol.TypeArguments.Any())
        {
            throw new InvalidOperationException($"Function provider class '{typeSymbol.Name}' must not have generic arguments");
        }

        var typeAuthorizationLevel = typeSymbol.GetAuthorizationLevel();

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "EndpointFunction",
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: typeSymbol.GetMembers().OfType<IMethodSymbol>().Select(InnerGetResolverMetadata).NotNull().ToArray());

        static bool IsFunctionProviderAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionProviderAttribute") is true;

        EndpointResolverMetadata? InnerGetResolverMetadata(IMethodSymbol methodSymbol)
            =>
            GetResolverMetadata(methodSymbol, typeAuthorizationLevel);
    }

    private static EndpointResolverMetadata? GetResolverMetadata(IMethodSymbol methodSymbol, int? typeAuthorizationLevel)
    {
        var functionAttribute = methodSymbol.GetAttributes().FirstOrDefault(IsFunctionAttribute);
        if (functionAttribute is null)
        {
            return null;
        }

        if (methodSymbol.IsStatic is false)
        {
            throw methodSymbol.CreateInvalidMethodException("must be static");
        }

        if (methodSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            throw methodSymbol.CreateInvalidMethodException("must be public or internal");
        }

        if (methodSymbol.Parameters.Any())
        {
            throw methodSymbol.CreateInvalidMethodException("must not have parameters");
        }

        if (methodSymbol.TypeParameters.Any())
        {
            throw methodSymbol.CreateInvalidMethodException("must not have generic arguments");
        }

        var endpointType = methodSymbol.GetEndpointTypeOrThrow();
        var name = methodSymbol.Name.RemoveStandardStart();

        var endpointAttribute = endpointType.GetAttributes().FirstOrDefault(IsEndpointMetadataAttribute);

        return new(
            endpointType: endpointType.GetDisplayedData(),
            resolverMethodName: methodSymbol.Name,
            functionMethodName: name.RemoveStandardEnd().SetLastWordAsFirst() + "Async",
            dependencyFieldName: name.FromLowerCase() + "Dependency",
            functionName: functionAttribute.GetAttributeValue(0, "Name")?.ToString() ?? string.Empty,
            authorizationLevel: methodSymbol.GetAuthorizationLevel() ?? typeAuthorizationLevel ?? default,
            httpMethodNames: endpointAttribute?.GetHttpMethodNames(),
            httpRoute: endpointAttribute?.GetHttpRoute(),
            obsoleteData: endpointType.GetObsoleteData() ?? methodSymbol.GetObsoleteData());

        static bool IsFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionAttribute") is true;

        static bool IsEndpointMetadataAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(EndpointNamespace, "EndpointMetadataAttribute") is true;
    }

    private static ObsoleteData? GetObsoleteData(this ISymbol symbol)
    {
        var obsoleteAttributeData = symbol.GetAttributes().FirstOrDefault(IsObsoleteAttribute);
        if (obsoleteAttributeData is null)
        {
            return null;
        }

        return new(
            message: obsoleteAttributeData.GetAttributeValue(0)?.ToString(),
            isError: obsoleteAttributeData.GetAttributeValue(1) as bool?,
            diagnosticId: obsoleteAttributeData.GetAttributePropertyValue("DiagnosticId")?.ToString(),
            urlFormat: obsoleteAttributeData.GetAttributePropertyValue("UrlFormat")?.ToString());

        static bool IsObsoleteAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsSystemType("ObsoleteAttribute") is true;
    }

    private static INamedTypeSymbol GetEndpointTypeOrThrow(this IMethodSymbol resolverMethod)
    {
        var returnType = resolverMethod.ReturnType as INamedTypeSymbol;
        if (returnType?.IsType("PrimeFuncPack", "Dependency") is not true || returnType?.TypeArguments.Length is not 1)
        {
            throw resolverMethod.CreateInvalidMethodException("return type must be PrimeFuncPack.Dependency<TEndpoint>");
        }

        var endpointType = returnType.TypeArguments[0] as INamedTypeSymbol;
        if (endpointType?.AllInterfaces.Any(IsEndpointType) is not true)
        {
            throw resolverMethod.CreateInvalidMethodException($"must resolve a type that implements {EndpointNamespace}.IEndpoint");
        }

        return endpointType;

        static bool IsEndpointType(INamedTypeSymbol typeSymbol)
            =>
            typeSymbol.IsType(EndpointNamespace, "IEndpoint");
    }

    private static int? GetAuthorizationLevel(this ISymbol symbol)
    {
        var authorizationAttribute = symbol.GetAttributes().FirstOrDefault(IsSecurityAttribute);
        if (authorizationAttribute is null)
        {
            return null;
        }

        var levelValue = authorizationAttribute.GetAttributeValue(0, "Level");
        if (levelValue is null)
        {
            return null;
        }

        if (levelValue is not int level)
        {
            throw new InvalidOperationException($"An unexpected endpoint function authorization level: {levelValue}");
        }

        return level;

        static bool IsSecurityAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionSecurityAttribute") is true;
    }

    private static IReadOnlyCollection<string>? GetHttpMethodNames(this AttributeData endpointAttribute)
    {
        var method = endpointAttribute.GetAttributeValue(0)?.ToString();
        if (string.IsNullOrEmpty(method))
        {
            return null;
        }

        return new[] { method ?? string.Empty };
    }

    private static string? GetHttpRoute(this AttributeData endpointAttribute)
    {
        var route = endpointAttribute.GetAttributeValue(1)?.ToString();
        if (string.IsNullOrEmpty(route))
        {
            return null;
        }

        if (route?.StartsWith("/", StringComparison.InvariantCulture) is true)
        {
            return route?.Substring(1);
        }

        return route;
    }
}