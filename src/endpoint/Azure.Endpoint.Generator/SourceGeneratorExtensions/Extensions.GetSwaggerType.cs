using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static FunctionSwaggerMetadata? GetFunctionSwaggerType(
        this Compilation compilation, CancellationToken cancellationToken)
    {
        var visitor = new ExportedTypesCollector(cancellationToken);
        visitor.VisitNamespace(compilation.GlobalNamespace);

        var swaggerTypes = visitor.GetExportedTypes().Select(GetSwaggerMetadata).NotNull().ToArray();

        if (swaggerTypes.Any() is false)
        {
            return null;
        }

        if (swaggerTypes.Length > 1)
        {
            throw new InvalidOperationException("There must be the only one function swagger type");
        }

        return swaggerTypes[0];
    }

    private static FunctionSwaggerMetadata? GetSwaggerMetadata(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.GetAttributes().Any(IsFunctionSwaggerAttribute) is false)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "Swagger");

        static bool IsFunctionSwaggerAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionSwaggerAttribute") is true;
    }
}