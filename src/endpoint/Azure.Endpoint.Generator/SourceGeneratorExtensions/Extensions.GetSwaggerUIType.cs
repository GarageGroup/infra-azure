using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static FunctionSwaggerUIMetadata? GetFunctionSwaggerUIType(
        this Compilation compilation, CancellationToken cancellationToken)
    {
        var visitor = new ExportedTypesCollector(cancellationToken);
        visitor.VisitNamespace(compilation.GlobalNamespace);

        var swaggerUITypes = visitor.GetExportedTypes().Select(GetFunctionSwaggerUIMetadata).NotNull().ToArray();
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