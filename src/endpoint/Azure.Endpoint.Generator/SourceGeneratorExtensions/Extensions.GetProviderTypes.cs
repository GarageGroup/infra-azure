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

        return visitor.GetExportedTypes().Select(GetFunctionMetadata).NotNull().ToArray();
    }

    private static FunctionProviderMetadata? GetFunctionMetadata(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeArguments.Any())
        {
            return null;
        }

        var typeAuthorizationLevel = typeSymbol.GetAuthorizationLevel();
        var resolverTypes = typeSymbol.GetMembers().OfType<IMethodSymbol>().Select(InnerGetResolverMetadata).NotNull().ToArray();

        if (resolverTypes.Length is 0)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "EndpointFunction",
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: resolverTypes);

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

        if (methodSymbol.TypeParameters.Any())
        {
            throw methodSymbol.CreateInvalidMethodException("must not have generic arguments");
        }

        var endpointType = methodSymbol.GetEndpointTypeOrThrow();
        var name = methodSymbol.Name.RemoveStandardStart();

        var endpointAttribute = endpointType.GetAttributes().FirstOrDefault(IsEndpointMetadataAttribute);
        var authorizationLevel = methodSymbol.GetAuthorizationLevel() ?? typeAuthorizationLevel ?? default;

        var defaultArguments = BuildDefaultArguments(authorizationLevel, endpointAttribute).ToDictionary(GetTypeName);
        var parameterArguments = methodSymbol.Parameters.Select(GetArgumentMetadata).ToArray();

        var arguments = new List<FunctionArgumentMetadata>(parameterArguments.Length);
        foreach (var parameterArgument in parameterArguments)
        {
            if (defaultArguments.TryGetValue(parameterArgument.TypeDisplayName, out var defaultArgument) is false)
            {
                arguments.Add(parameterArgument);
                continue;
            }

            var argument = new FunctionArgumentMetadata(
                namespaces: parameterArgument.Namespaces,
                typeDisplayName: parameterArgument.TypeDisplayName,
                argumentName: defaultArgument.ArgumentName,
                orderNumber: defaultArgument.OrderNumber,
                extensionMethodArgumentOrder: defaultArgument.ExtensionMethodArgumentOrder,
                resolverMethodArgumentOrder: parameterArgument.ResolverMethodArgumentOrder,
                attributes: parameterArgument.Attributes);

            arguments.Add(argument);
            defaultArguments.Remove(parameterArgument.TypeDisplayName);
        }

        return new(
            endpointType: endpointType.GetDisplayedData(),
            resolverMethodName: methodSymbol.Name,
            functionMethodName: name.RemoveStandardEnd().SetLastWordAsFirst() + "Async",
            dependencyFieldName: name.FromLowerCase() + "Dependency",
            functionName: functionAttribute.GetAttributeValue(0, "Name")?.ToString() ?? string.Empty,
            obsoleteData: endpointType.GetObsoleteData() ?? methodSymbol.GetObsoleteData(),
            arguments: [.. arguments, .. defaultArguments.Values],
            isSwaggerHidden: functionAttribute.GetAttributePropertyValue("IsSwaggerHidden") is true);

        static bool IsFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionAttribute") is true;

        static bool IsEndpointMetadataAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(EndpointNamespace, "EndpointMetadataAttribute") is true;

        static string GetTypeName(FunctionArgumentMetadata argument)
            =>
            argument.TypeDisplayName;
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

    private static IReadOnlyCollection<string> GetHttpMethodNames(this AttributeData? endpointAttribute)
    {
        var method = endpointAttribute?.GetAttributeValue(0)?.ToString();
        if (string.IsNullOrEmpty(method))
        {
            return [];
        }

        return [method ?? string.Empty];
    }

    private static string? GetHttpRoute(this AttributeData? endpointAttribute)
    {
        var route = endpointAttribute?.GetAttributeValue(1)?.ToString();
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

    private static FunctionArgumentMetadata GetArgumentMetadata(IParameterSymbol parameter, int order)
    {
        var type = parameter.Type.GetDisplayedData();

        return new(
            namespaces: type.AllNamespaces.ToArray(),
            typeDisplayName: type.DisplayedTypeName,
            argumentName: parameter.Name,
            orderNumber: order,
            extensionMethodArgumentOrder: null,
            resolverMethodArgumentOrder: order,
            attributes: parameter.GetAttributes().Select(GetAttributeMetadata).NotNull().ToArray());
    }

    private static FunctionAttributeMetadata? GetAttributeMetadata(AttributeData attribute)
    {
        var type = attribute.AttributeClass?.GetDisplayedData();
        if (type is null)
        {
            return null;
        }

        var namespaces = type.AllNamespaces.ToList();

        return new(
            namespaces: namespaces,
            typeDisplayName: type.DisplayedTypeName,
            constructorArgumentSourceCodes: attribute.ConstructorArguments.Select(BuildArgumentSourceCode).ToArray(),
            propertySourceCodes: attribute.NamedArguments.Select(BuildPropertySourceCode).ToArray());

        KeyValuePair<string, string> BuildPropertySourceCode(KeyValuePair<string, TypedConstant> namedArgument)
            =>
            new(namedArgument.Key, BuildArgumentSourceCode(namedArgument.Value));

        string BuildArgumentSourceCode(TypedConstant argument)
        {
            if (argument.Value is null)
            {
                return "null";
            }

            if (argument.Value is string stringValue)
            {
                return stringValue.AsStringSourceCodeOr();
            }

            if (argument.Type?.GetEnumUnderlyingTypeOrDefault() is not null)
            {
                var enumType = argument.Type.GetDisplayedData();
                namespaces.AddRange(enumType.AllNamespaces);
                return $"({enumType.DisplayedTypeName}){argument.Value}";
            }

            return argument.Value.ToString();
        }
    }
}