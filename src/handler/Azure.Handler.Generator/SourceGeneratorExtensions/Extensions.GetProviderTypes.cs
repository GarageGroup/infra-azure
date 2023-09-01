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
            typeName: typeSymbol.Name + "HandlerFunction",
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: resolverTypes);
    }

    private static IReadOnlyList<HandlerResolverMetadata> GetResolversMetadata(IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Where(IsFunctionAttribute).Select(InnerGetResolverMetadata).ToArray();

        static bool IsFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.BaseType?.IsType(DefaultNamespace, "HandlerFunctionAttribute") is true;

        HandlerResolverMetadata InnerGetResolverMetadata(AttributeData functionAttribute)
            =>
            GetResolverMetadata(methodSymbol, functionAttribute);
    }

    private static HandlerResolverMetadata GetResolverMetadata(this IMethodSymbol methodSymbol, AttributeData functionAttribute)
    {
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

        var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
        if (returnType?.IsType("PrimeFuncPack", "Dependency") is not true || returnType?.TypeArguments.Length is not 1)
        {
            throw methodSymbol.CreateInvalidMethodException("return type must be PrimeFuncPack.Dependency<THandler>");
        }

        var handlerType = returnType.TypeArguments[0] as INamedTypeSymbol;
        var handlerInterface = handlerType?.AllInterfaces.FirstOrDefault(IsHandlerType);

        var inputType = handlerInterface?.TypeArguments.FirstOrDefault();
        var outputType = handlerInterface?.TypeArguments.ElementAtOrDefault(1);

        if (handlerType is null || inputType is null || outputType is null)
        {
            throw methodSymbol.CreateInvalidMethodException($"must resolve a type that implements {DefaultNamespace}.IHandler<TIn, TOut>");
        }

        var functionName = functionAttribute.GetAttributeValue(0, "Name")?.ToString() ?? methodSymbol.Name;

        return new(
            handlerType: handlerType.GetDisplayedData(),
            inputType: inputType.GetDisplayedData(),
            outputType: outputType.GetDisplayedData(),
            resolverMethodName: methodSymbol.Name,
            functionMethodName: functionName + "Async",
            functionName: functionName,
            jsonRootPath: inputType.GetJsonRootPath(),
            functionData: functionAttribute.GetFunctionData());

        static bool IsHandlerType(INamedTypeSymbol typeSymbol)
            =>
            typeSymbol.IsType(DefaultNamespace, "IHandler") && typeSymbol.TypeParameters.Length is 2;
    }

    private static BaseFunctionData GetFunctionData(this AttributeData functionAttribute)
    {
        if (functionAttribute.AttributeClass?.IsType(DefaultNamespace, "EventGridFunctionAttribute") is true)
        {
            return new EventGridFunctionData();
        }

        if (functionAttribute.AttributeClass?.IsType(DefaultNamespace, "HttpFunctionAttribute") is true)
        {
            return new HttpFunctionData(
                method: functionAttribute.GetAttributeValue(1)?.ToString() ?? string.Empty,
                functionRoute: functionAttribute.GetAttributePropertyValue("Route")?.ToString(),
                authorizationLevel: functionAttribute.GetAuthorizationLevel());
        }

        if (functionAttribute.AttributeClass?.IsType(DefaultNamespace, "ServiceBusFunctionAttribute") is true)
        {
            return functionAttribute.GetServiceBusFunctionData();
        }

        throw new InvalidOperationException($"An unexpected HandlerFunctionAttribute type: '{functionAttribute.AttributeClass?.Name}'");
    }

    private static string? GetJsonRootPath(this ITypeSymbol eventDataType)
    {
        return eventDataType.GetAttributes().FirstOrDefault(IsHandlerDataJsonAttribute)?.GetAttributeValue(0)?.ToString();

        static bool IsHandlerDataJsonAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HandlerDataJsonAttribute") is true;
    }

    private static int GetAuthorizationLevel(this AttributeData functionAttribute)
    {
        var levelValue = functionAttribute.GetAttributePropertyValue("AuthLevel");
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

    private static ServiceBusFunctionData GetServiceBusFunctionData(this AttributeData data)
        =>
        data.ConstructorArguments.Length switch
        {
            QueueServiceBusConstructorArgumentCount => new ServiceBusFunctionData(
                queueName: data.GetAttributeValue(1)?.ToString(),
                connection: data.GetAttributeValue(2)?.ToString()),
            SubscriptionServiceBusConstructorArgumentCount => new ServiceBusFunctionData(
                topicName: data.GetAttributeValue(1)?.ToString(),
                subscriptionName: data.GetAttributeValue(2)?.ToString(),
                connection: data.GetAttributeValue(3)?.ToString()),
            _ => throw new ArgumentOutOfRangeException($"There is no ServiceBusFunctionData that takes {data.ConstructorArguments.Length} arguments"),
        };
}