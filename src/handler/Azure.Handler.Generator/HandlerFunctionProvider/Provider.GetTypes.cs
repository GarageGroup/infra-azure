using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

partial class HandlerFunctionProvider
{
    public IReadOnlyCollection<FunctionProviderMetadata> GetFunctionProviderTypes(GeneratorExecutionContext context)
    {
        var visitor = new ExportedTypesCollector(context.CancellationToken);
        visitor.VisitNamespace(context.Compilation.GlobalNamespace);

        return visitor.GetNonPrivateTypes().Select(GetFunctionMetadata).NotNull().ToArray();
    }

    private FunctionProviderMetadata? GetFunctionMetadata(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeArguments.Any())
        {
            return null;
        }

        var resolverTypes = typeSymbol.GetMembers().OfType<IMethodSymbol>().SelectMany(GetResolversMetadata).ToArray();
        if (resolverTypes.Any() is false)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + typeNameSuffix,
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: resolverTypes);
    }

    private IReadOnlyList<HandlerResolverMetadata> GetResolversMetadata(IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Where(IsFunctionAttribute).Select(InnerGetResolverMetadata).NotNull().ToArray();

        static bool IsFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.BaseType?.IsType(DefaultNamespace, "HandlerFunctionAttribute") is true;

        HandlerResolverMetadata? InnerGetResolverMetadata(AttributeData functionAttribute)
            =>
            GetResolverMetadata(methodSymbol, functionAttribute);
    }

    private HandlerResolverMetadata? GetResolverMetadata(IMethodSymbol methodSymbol, AttributeData functionAttribute)
    {
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
            throw methodSymbol.CreateInvalidMethodException("must have no generic arguments");
        }

        var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
        if (returnType?.IsType("PrimeFuncPack", "Dependency") is not true || returnType?.TypeArguments.Length is not 1)
        {
            throw methodSymbol.CreateInvalidMethodException("return type must be PrimeFuncPack.Dependency<THandler>");
        }

        var handlerType = returnType.TypeArguments[0] as INamedTypeSymbol;
        var handlerInterface = IsHandlerType(handlerType) ? handlerType : handlerType?.AllInterfaces.FirstOrDefault(IsHandlerType);

        var inputType = handlerInterface?.TypeArguments.FirstOrDefault();
        var outputType = handlerInterface?.TypeArguments.ElementAtOrDefault(1);

        if (handlerType is null || inputType is null || outputType is null)
        {
            throw methodSymbol.CreateInvalidMethodException($"must resolve a type that implements {DefaultNamespace}.IHandler<TIn, TOut>");
        }

        var functionDataContext = new FunctionDataContext(
            handlerType: handlerType.GetDisplayedData(),
            inputType: inputType.GetDisplayedData(),
            outputType: outputType.GetDisplayedData());

        var functionSpecificData = GetFunctionSpecificData(methodSymbol, functionAttribute, functionDataContext);
        if (functionSpecificData is null)
        {
            return null;
        }

        var functionName = functionAttribute.GetAttributeValue(0, "Name")?.ToString() ?? methodSymbol.Name;

        return new(
            handlerType: functionDataContext.HandlerType,
            inputType: functionDataContext.InputType,
            outputType: functionDataContext.OutputType,
            resolverMethodName: methodSymbol.Name,
            functionMethodName: functionName + "Async",
            functionName: functionName,
            jsonRootPath: GetJsonRootPath(inputType),
            functionSpecificData: functionSpecificData);

        static bool IsHandlerType(INamedTypeSymbol? typeSymbol)
            =>
            typeSymbol?.IsType(DefaultNamespace, "IHandler") is true && typeSymbol.TypeParameters.Length is 2;
    }

    private HandlerFunctionMetadata? GetFunctionSpecificData(
        IMethodSymbol methodSymbol, AttributeData functionAttribute, FunctionDataContext context)
    {
        var functionMetadata = functionDataProviders.Select(InnerGetFunctionMetadata).NotNull().FirstOrDefault();

        if (functionMetadata is null)
        {
            return null;
        }

        if (methodSymbol.Parameters.Any() is false)
        {
            return functionMetadata;
        }

        var defaultArguments = functionMetadata.Arguments.ToDictionary(GetTypeName);
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
            namespaces: functionMetadata.Namespaces,
            responseTypeDisplayName: functionMetadata.ResponseTypeDisplayName,
            extensionsMethodName: functionMetadata.ExtensionsMethodName,
            arguments: arguments.Concat(defaultArguments.Values).ToArray());

        HandlerFunctionMetadata? InnerGetFunctionMetadata(IFunctionDataProvider provider)
            =>
            provider.GetFunctionMetadata(functionAttribute, context);

        static string GetTypeName(FunctionArgumentMetadata argument)
            =>
            argument.TypeDisplayName;
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

    private static string? GetJsonRootPath(ITypeSymbol eventDataType)
    {
        return eventDataType.GetAttributes().FirstOrDefault(IsHandlerDataJsonAttribute)?.GetAttributeValue(0)?.ToString();

        static bool IsHandlerDataJsonAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HandlerDataJsonAttribute") is true;
    }
}