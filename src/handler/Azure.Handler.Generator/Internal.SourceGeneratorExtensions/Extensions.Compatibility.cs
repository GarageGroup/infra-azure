using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    internal static object? GetAttributeValue(this AttributeData attributeData, int constructorArgumentOrder)
    {
        if (constructorArgumentOrder < 0 || constructorArgumentOrder >= attributeData.ConstructorArguments.Length)
        {
            return null;
        }

        return attributeData.ConstructorArguments[constructorArgumentOrder].Value;
    }

    internal static object? GetAttributeValue(this AttributeData attributeData, int constructorArgumentOrder, string propertyName)
        =>
        attributeData.GetAttributeValue(constructorArgumentOrder) ?? attributeData.GetAttributePropertyValue(propertyName);

    internal static object? GetAttributePropertyValue(this AttributeData attributeData, string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return null;
        }

        foreach (var namedArgument in attributeData.NamedArguments)
        {
            if (string.Equals(namedArgument.Key, propertyName, StringComparison.Ordinal))
            {
                return namedArgument.Value.Value;
            }
        }

        return null;
    }

    internal static ITypeSymbol? GetEnumUnderlyingTypeOrDefault(this ITypeSymbol typeSymbol)
        =>
        (typeSymbol as INamedTypeSymbol)?.EnumUnderlyingType;
}
