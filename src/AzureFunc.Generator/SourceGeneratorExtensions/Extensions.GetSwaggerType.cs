using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GGroupp.Infra;

partial class SourceGeneratorExtensions
{
    internal static FunctionSwaggerMetadata? GetFunctionSwaggerType(this GeneratorExecutionContext context)
    {
        var visitor = new ExportedTypesCollector(context.CancellationToken);
        visitor.VisitNamespace(context.Compilation.GlobalNamespace);

        var swaggerTypes = visitor.GetNonPrivateTypes().Select(GetSwaggerMetadata).NotNull().ToArray();

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