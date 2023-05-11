using System;
using System.Collections.Generic;
using System.Linq;
using GGroupp;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    internal static IReadOnlyCollection<HealthCheckMetadata> GetHealthCheckTypes(this GeneratorExecutionContext context)
    {
        var visitor = new ExportedTypesCollector(context.CancellationToken);
        visitor.VisitNamespace(context.Compilation.GlobalNamespace);

        return visitor.GetNonPrivateTypes().Select(GetFunctionMetadata).NotNull().ToArray();
    }

    private static HealthCheckMetadata? GetFunctionMetadata(INamedTypeSymbol typeSymbol)
    {
        var functionAttribute = typeSymbol.GetAttributes().FirstOrDefault(IsHealthCheckFuncAttribute);
        if (functionAttribute is null)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "HealthCheckFunction",
            functionName: functionAttribute.GetAttributeValue(0)?.ToString() ?? string.Empty,
            functionRoute: functionAttribute.GetAttributeValue(1)?.ToString() ?? string.Empty,
            authorizationLevel: functionAttribute.GetAuthorizationLevel());

        static bool IsHealthCheckFuncAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HealthCheckFuncAttribute") is true;
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
}