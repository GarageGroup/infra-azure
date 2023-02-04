using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GGroupp.Infra;

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

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "HandlerFunction",
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: typeSymbol.GetMembers().OfType<IMethodSymbol>().Select(GetResolverMetadata).NotNull().ToArray());

        static bool IsFunctionProviderAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HandlerFunctionProviderAttribute") is true;
    }

    private static HandlerResolverMetadata? GetResolverMetadata(IMethodSymbol methodSymbol)
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

        var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
        if (returnType?.IsType("PrimeFuncPack", "Dependency") is not true || returnType?.TypeArguments.Length is not 1)
        {
            throw methodSymbol.CreateInvalidMethodException("return type must be PrimeFuncPack.Dependency<THandler>");
        }

        var handlerType = returnType.TypeArguments[0] as INamedTypeSymbol;
        var eventDataType = handlerType?.AllInterfaces.FirstOrDefault(IsHandlerType)?.TypeArguments.FirstOrDefault();
        if (handlerType is null || eventDataType is null)
        {
            throw methodSymbol.CreateInvalidMethodException($"must resolve a type that implements {DefaultNamespace}.IHandler<TEventData>");
        }

        var name = methodSymbol.Name.RemoveStandardStart();

        return new(
            handlerType: handlerType.GetDisplayedData(),
            evendDataType: eventDataType.GetDisplayedData(),
            functionAttributeTypeName: "EventGridTrigger",
            functionAttributeNamespace: "Microsoft.Azure.Functions.Worker",
            resolverMethodName: methodSymbol.Name,
            functionMethodName: name.ReplaceStandardEnd().SetLastWordAsFirst() + "Async",
            dependencyFieldName: name.FromLowerCase() + "Dependency",
            functionName: functionAttribute.GetAttributeValue(0, "Name")?.ToString() ?? string.Empty,
            jsonRootPath: eventDataType.GetJsonRootPath());

        static bool IsFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EventGridFunctionAttribute") is true;

        static bool IsHandlerType(INamedTypeSymbol typeSymbol)
            =>
            typeSymbol.IsType(DefaultNamespace, "IHandler") && typeSymbol.TypeParameters.Length is 1;
    }

    private static string? GetJsonRootPath(this ITypeSymbol eventDataType)
    {
        return eventDataType.GetAttributes().FirstOrDefault(IsHandlerDataJsonAttribute)?.GetAttributeValue(0)?.ToString();

        static bool IsHandlerDataJsonAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HandlerDataJsonAttribute") is true;
    }
}