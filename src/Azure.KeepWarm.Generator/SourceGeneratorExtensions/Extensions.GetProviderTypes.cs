using System.Collections.Generic;
using System.Linq;
using GGroupp;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    internal static IReadOnlyCollection<KeepWarmMetadata> GetKeepWarmTypes(this GeneratorExecutionContext context)
    {
        var visitor = new ExportedTypesCollector(context.CancellationToken);
        visitor.VisitNamespace(context.Compilation.GlobalNamespace);

        return visitor.GetNonPrivateTypes().Select(GetFunctionMetadata).NotNull().ToArray();
    }

    private static KeepWarmMetadata? GetFunctionMetadata(INamedTypeSymbol typeSymbol)
    {
        var functionAttribute = typeSymbol.GetAttributes().FirstOrDefault(IsKeepWarmFunctionAttribute);
        if (functionAttribute is null)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "WarmFunction",
            functionName: functionAttribute.GetAttributeValue(0)?.ToString() ?? string.Empty,
            functionSchedule: functionAttribute.GetAttributeValue(1)?.ToString() ?? string.Empty);

        static bool IsKeepWarmFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "KeepWarmFunctionAttribute") is true;
    }
}