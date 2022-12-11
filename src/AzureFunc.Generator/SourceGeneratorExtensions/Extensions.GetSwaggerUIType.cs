using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GGroupp.Infra;

partial class SourceGeneratorExtensions
{
    internal static FunctionSwaggerUIMetadata? GetFunctionSwaggerUIType(this GeneratorExecutionContext context)
    {
        var visitor = new ExportedTypesCollector(context.CancellationToken);
        visitor.VisitNamespace(context.Compilation.GlobalNamespace);

        var swaggerUITypes = visitor.GetNonPrivateTypes().Select(GetFunctionSwaggerUIMetadata).NotNull().ToArray();

        if (swaggerUITypes.Any() is false)
        {
            return null;
        }

        if (swaggerUITypes.Length > 1)
        {
            throw new InvalidOperationException("There must be the only one function swagger UI type");
        }

        return swaggerUITypes[0];
    }

    private static FunctionSwaggerUIMetadata? GetFunctionSwaggerUIMetadata(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.GetAttributes().Any(IsFunctionSwaggerAttribute) is false)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "SwaggerUI");

        static bool IsFunctionSwaggerAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionSwaggerUIAttribute") is true;
    }
}