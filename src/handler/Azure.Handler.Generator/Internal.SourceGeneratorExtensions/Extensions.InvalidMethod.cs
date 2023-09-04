using System;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static InvalidOperationException CreateInvalidMethodException(this IMethodSymbol resolverMethod, string message)
        =>
        new($"Function resolver method {resolverMethod.ContainingType?.Name}.{resolverMethod.Name} {message}");
}