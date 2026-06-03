using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    internal static IReadOnlyCollection<FunctionProviderMetadata> GetFunctionProviderTypes(
        this Compilation compilation, CancellationToken cancellationToken)
    {
        var visitor = new ExportedTypesCollector(cancellationToken);
        visitor.VisitNamespace(compilation.GlobalNamespace);

        return visitor.GetExportedTypes().Select(GetFunctionMetadata).NotNull().ToArray();
    }

    private static FunctionProviderMetadata? GetFunctionMetadata(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeArguments.Any())
        {
            return null;
        }

        var typeAuthorizationLevel = typeSymbol.GetAuthorizationLevel();
        var resolverTypes = typeSymbol.GetMembers().OfType<IMethodSymbol>().SelectMany(InnerGetResolverMetadata).ToArray();

        if (resolverTypes.Length is 0)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "EndpointFunction",
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: resolverTypes);

        IReadOnlyCollection<EndpointResolverMetadata> InnerGetResolverMetadata(IMethodSymbol methodSymbol)
            =>
            GetResolverMetadata(methodSymbol, typeAuthorizationLevel);
    }

    private static IReadOnlyCollection<EndpointResolverMetadata> GetResolverMetadata(IMethodSymbol methodSymbol, int? typeAuthorizationLevel)
    {
        var functionAttribute = methodSymbol.GetAttributes().FirstOrDefault(IsEndpointFunctionAttribute);
        var setFunctionAttribute = methodSymbol.GetAttributes().FirstOrDefault(IsEndpointSetFunctionAttribute);

        if (functionAttribute is null && setFunctionAttribute is null)
        {
            return [];
        }

        if (functionAttribute is not null && setFunctionAttribute is not null)
        {
            throw methodSymbol.CreateInvalidMethodException(
                $"must have only one of {DefaultNamespace}.EndpointFunctionAttribute or {DefaultNamespace}.EndpointSetFunctionAttribute");
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

        return functionAttribute switch
        {
            not null => [GetEndpointResolverMetadata(methodSymbol, functionAttribute, typeAuthorizationLevel)],
            _ => GetEndpointSetResolverMetadata(methodSymbol, setFunctionAttribute!, typeAuthorizationLevel)
        };

        static bool IsEndpointFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionAttribute") is true;

        static bool IsEndpointSetFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointSetFunctionAttribute") is true;
    }

    private static EndpointResolverMetadata GetEndpointResolverMetadata(
        IMethodSymbol methodSymbol, AttributeData functionAttribute, int? typeAuthorizationLevel)
    {
        var endpointType = methodSymbol.GetEndpointTypeOrThrow();
        var name = methodSymbol.Name.RemoveStandardStart();

        var endpointMetadataAttribute = endpointType.GetAttributes().FirstOrDefault(IsEndpointMetadataAttribute);
        var endpointOperationMetadataAttribute = endpointType.GetAttributes().FirstOrDefault(IsEndpointOperationMetadataAttribute);
        var functionName = methodSymbol.GetFunctionNameOrThrow(
            functionAttribute,
            endpointMetadataAttribute,
            endpointOperationMetadataAttribute);

        var authorizationLevel = methodSymbol.GetAuthorizationLevel() ?? typeAuthorizationLevel ?? DefaultFunctionAuthorizationLevel;

        var defaultArguments = BuildDefaultArguments(
            authorizationLevel,
            endpointOperationMetadataAttribute ?? endpointMetadataAttribute).ToDictionary(GetTypeName);
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
            functionName: functionName,
            obsoleteData: endpointType.GetObsoleteData() ?? methodSymbol.GetObsoleteData(),
            arguments: [.. arguments, .. defaultArguments.Values],
            isAuthorizationRequired: authorizationLevel is not AnonymousFunctionAuthorizationLevel,
            isSwaggerHidden: functionAttribute.GetNamedArgumentValue<bool?>("IsSwaggerHidden") is true,
            isEndpointSetOperation: false,
            endpointOperationId: null);

        static string GetTypeName(FunctionArgumentMetadata argument)
            =>
            argument.TypeDisplayName;
    }

    private static IReadOnlyCollection<EndpointResolverMetadata> GetEndpointSetResolverMetadata(
        IMethodSymbol methodSymbol, AttributeData setFunctionAttribute, int? typeAuthorizationLevel)
    {
        var endpointSetType = methodSymbol.GetEndpointSetTypeOrThrow();
        var operationMetadataAttributes = endpointSetType.GetAttributes().Where(IsEndpointOperationMetadataAttribute).ToArray();

        if (operationMetadataAttributes.Length is 0)
        {
            throw methodSymbol.CreateInvalidMethodException(
                $"must resolve a type that has at least one {EndpointNamespace}.EndpointOperationMetadataAttribute");
        }

        var name = methodSymbol.Name.RemoveStandardStart();
        var authorizationLevel = methodSymbol.GetAuthorizationLevel() ?? typeAuthorizationLevel ?? DefaultFunctionAuthorizationLevel;
        var parameterArguments = methodSymbol.Parameters.Select(GetArgumentMetadata).ToArray();
        var usedMethodNames = new HashSet<string>(StringComparer.Ordinal);

        var resolverMetadata = new List<EndpointResolverMetadata>(operationMetadataAttributes.Length);

        foreach (var operationMetadataAttribute in operationMetadataAttributes)
        {
            var operationId = operationMetadataAttribute.GetOperationId();
            if (string.IsNullOrWhiteSpace(operationId))
            {
                throw methodSymbol.CreateInvalidMethodException(
                    $"{EndpointNamespace}.EndpointOperationMetadataAttribute operationId must not be null or whitespace");
            }

            var endpointOperationId = operationId!;
            var defaultArguments = BuildDefaultArguments(authorizationLevel, operationMetadataAttribute).ToDictionary(GetTypeName);
            var arguments = new List<FunctionArgumentMetadata>(parameterArguments.Length);

            foreach (var parameterArgument in parameterArguments)
            {
                if (defaultArguments.TryGetValue(parameterArgument.TypeDisplayName, out var defaultArgument) is false)
                {
                    arguments.Add(parameterArgument);
                    continue;
                }

                arguments.Add(
                    new(
                        namespaces: parameterArgument.Namespaces,
                        typeDisplayName: parameterArgument.TypeDisplayName,
                        argumentName: defaultArgument.ArgumentName,
                        orderNumber: defaultArgument.OrderNumber,
                        extensionMethodArgumentOrder: defaultArgument.ExtensionMethodArgumentOrder,
                        resolverMethodArgumentOrder: parameterArgument.ResolverMethodArgumentOrder,
                        attributes: parameterArgument.Attributes));

                defaultArguments.Remove(parameterArgument.TypeDisplayName);
            }

            var methodName = endpointOperationId.BuildFunctionMethodName();
            if (usedMethodNames.Add(methodName) is false)
            {
                var index = resolverMetadata.Count + 1;
                methodName = $"{methodName}{index}";
            }

            resolverMetadata.Add(
                new(
                    endpointType: endpointSetType.GetDisplayedData(),
                    resolverMethodName: methodSymbol.Name,
                    functionMethodName: methodName + "Async",
                    dependencyFieldName: name.FromLowerCase() + "Dependency",
                    functionName: endpointOperationId,
                    obsoleteData: endpointSetType.GetObsoleteData() ?? methodSymbol.GetObsoleteData(),
                    arguments: [.. arguments, .. defaultArguments.Values],
                    isAuthorizationRequired: authorizationLevel is not AnonymousFunctionAuthorizationLevel,
                    isSwaggerHidden: setFunctionAttribute.GetNamedArgumentValue<bool?>("IsSwaggerHidden") is true,
                    isEndpointSetOperation: true,
                    endpointOperationId: endpointOperationId));
        }

        return resolverMetadata;

        static string GetTypeName(FunctionArgumentMetadata argument)
            =>
            argument.TypeDisplayName;
    }

    private static string BuildFunctionMethodName(this string operationId)
    {
        if (string.IsNullOrWhiteSpace(operationId))
        {
            return "Invoke";
        }

        var isPreviousSeparator = true;
        var builder = new StringBuilder(operationId.Length);

        foreach (var character in operationId)
        {
            if (char.IsLetterOrDigit(character) is false)
            {
                isPreviousSeparator = true;
                continue;
            }

            var outputCharacter = isPreviousSeparator ? char.ToUpperInvariant(character) : character;
            builder.Append(outputCharacter);

            isPreviousSeparator = false;
        }

        if (builder.Length is 0)
        {
            return "Invoke";
        }

        if (char.IsDigit(builder[0]))
        {
            _ = builder.Insert(0, '_');
        }

        return builder.ToString();
    }

    private static INamedTypeSymbol GetEndpointSetTypeOrThrow(this IMethodSymbol resolverMethod)
    {
        var returnType = resolverMethod.ReturnType as INamedTypeSymbol;
        if (returnType?.IsType("PrimeFuncPack", "Dependency") is not true || returnType?.TypeArguments.Length is not 1)
        {
            throw resolverMethod.CreateInvalidMethodException("return type must be PrimeFuncPack.Dependency<TEndpointSet>");
        }

        var endpointSetType = returnType.TypeArguments[0] as INamedTypeSymbol;
        if (endpointSetType?.AllInterfaces.Any(IsEndpointSetType) is not true)
        {
            throw resolverMethod.CreateInvalidMethodException($"must resolve a type that implements {EndpointNamespace}.IEndpointSet");
        }

        return endpointSetType;

        static bool IsEndpointSetType(INamedTypeSymbol typeSymbol)
            =>
            typeSymbol.IsType(EndpointNamespace, "IEndpointSet");
    }

    private static bool IsEndpointMetadataAttribute(AttributeData attributeData)
        =>
        attributeData.AttributeClass?.IsType(EndpointNamespace, "EndpointMetadataAttribute") is true;

    private static bool IsEndpointOperationMetadataAttribute(AttributeData attributeData)
        =>
        attributeData.AttributeClass?.IsType(EndpointNamespace, "EndpointOperationMetadataAttribute") is true;

    private static ObsoleteData? GetObsoleteData(this ISymbol symbol)
    {
        var obsoleteAttributeData = symbol.GetAttributes().FirstOrDefault(IsObsoleteAttribute);
        if (obsoleteAttributeData is null)
        {
            return null;
        }

        return new(
            message: obsoleteAttributeData.GetConstructorArgumentValue<string?>(0),
            isError: obsoleteAttributeData.GetConstructorArgumentValue<bool?>(1),
            diagnosticId: obsoleteAttributeData.GetNamedArgumentValue<string?>("DiagnosticId"),
            urlFormat: obsoleteAttributeData.GetNamedArgumentValue<string?>("UrlFormat"));

        static bool IsObsoleteAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType("ObsoleteAttribute", "System") is true;
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
        return symbol.GetAttributes().FirstOrDefault(IsSecurityAttribute)?.GetConstructorArgumentValue<int>(0);

        static bool IsSecurityAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionSecurityAttribute") is true;
    }

    private static string GetFunctionNameOrThrow(
        this IMethodSymbol methodSymbol,
        AttributeData functionAttribute,
        AttributeData? endpointMetadataAttribute,
        AttributeData? endpointOperationMetadataAttribute)
    {
        var functionName = functionAttribute.GetConstructorArgumentValue<string?>(0);
        var operationId = endpointOperationMetadataAttribute.GetOperationId();

        if (string.IsNullOrWhiteSpace(functionName))
        {
            if (string.IsNullOrWhiteSpace(operationId))
            {
                throw methodSymbol.CreateInvalidMethodException(
                    $"must have function name in {DefaultNamespace}.EndpointFunctionAttribute(name) " +
                    $"or {EndpointNamespace}.EndpointOperationMetadataAttribute");
            }

            return operationId ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(operationId) is false)
        {
            return operationId ?? string.Empty;
        }

        if (endpointMetadataAttribute is not null)
        {
            return functionName ?? string.Empty;
        }

        return functionName ?? string.Empty;
    }

    private static string? GetOperationId(this AttributeData? endpointAttribute)
    {
        if (endpointAttribute?.AttributeClass?.IsType(EndpointNamespace, "EndpointOperationMetadataAttribute") is not true)
        {
            return null;
        }

        var operationId = endpointAttribute.GetConstructorArgumentValue<string?>(0);
        if (string.IsNullOrWhiteSpace(operationId))
        {
            return null;
        }

        return operationId;
    }

    private static IReadOnlyCollection<string> GetHttpMethodNames(this AttributeData? endpointAttribute)
    {
        var methodArgumentIndex = endpointAttribute switch
        {
            null => (int?)null,
            _ when endpointAttribute.AttributeClass?.IsType(EndpointNamespace, "EndpointOperationMetadataAttribute") is true => 1,
            _ when endpointAttribute.AttributeClass?.IsType(EndpointNamespace, "EndpointMetadataAttribute") is true => 0,
            _ => null
        };

        var method = endpointAttribute?.GetConstructorArgumentValue<string?>(methodArgumentIndex ?? 0);
        if (string.IsNullOrEmpty(method))
        {
            return [];
        }

        return [method ?? string.Empty];
    }

    private static string? GetHttpRoute(this AttributeData? endpointAttribute)
    {
        var routeArgumentIndex = endpointAttribute switch
        {
            null => (int?)null,
            _ when endpointAttribute.AttributeClass?.IsType(EndpointNamespace, "EndpointOperationMetadataAttribute") is true => 2,
            _ when endpointAttribute.AttributeClass?.IsType(EndpointNamespace, "EndpointMetadataAttribute") is true => 1,
            _ => null
        };

        var route = endpointAttribute?.GetConstructorArgumentValue<string?>(routeArgumentIndex ?? 0);
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

            if (argument.Type?.GetEnumUnderlyingType() is not null)
            {
                var enumType = argument.Type.GetDisplayedData();
                namespaces.AddRange(enumType.AllNamespaces);
                return $"({enumType.DisplayedTypeName}){argument.Value}";
            }

            return argument.Value.ToString();
        }
    }
}