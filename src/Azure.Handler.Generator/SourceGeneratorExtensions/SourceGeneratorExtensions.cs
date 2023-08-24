using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string DefaultNamespace = "GarageGroup.Infra";

    private const int QueueServiceBusConstructorArgumentCount = 3;

    private const int SubscriptionServiceBusConstructorArgumentCount = 4;

    internal static StringBuilder AppendSeparator(this StringBuilder builder, bool needSeparator)
        =>
        needSeparator ? builder.Append(", ") : builder;

    private static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item is null)
            {
                continue;
            }

            yield return item;
        }
    }

    private static InvalidOperationException CreateInvalidMethodException(this IMethodSymbol resolverMethod, string message)
        =>
        new($"Function resolver method {resolverMethod.ContainingType?.Name}.{resolverMethod.Name} {message}");
}