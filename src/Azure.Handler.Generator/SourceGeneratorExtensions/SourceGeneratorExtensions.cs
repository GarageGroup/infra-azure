using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string DefaultNamespace = "GarageGroup.Infra";

    private const string ResolverStandardStart = "Use";

    private const string ResolverStandardEnd = "Handler";

    private static string RemoveStandardStart(this string name)
    {
        var startLength = ResolverStandardStart.Length;
        if (name.Length <= startLength)
        {
            return name;
        }

        if (name.StartsWith(ResolverStandardStart, StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return name;
        }

        return name.Substring(startLength);
    }

    private static string ReplaceStandardEnd(this string name)
    {
        var endLength = ResolverStandardEnd.Length;
        if (name.Length <= endLength)
        {
            return name;
        }

        if (name.EndsWith(ResolverStandardEnd, StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return name;
        }

        return name.Substring(0, name.Length - endLength) + "Handle";
    }

    private static string SetLastWordAsFirst(this string name)
    {
        var lastCapital =  Array.FindLastIndex(name.ToCharArray(), char.IsUpper);
        if (lastCapital < 0)
        {
            return name;
        }

        return name.Substring(lastCapital) + name.Substring(0, lastCapital);
    }

    private static string FromLowerCase(this string name)
    {
        if (name.Length <= 1)
        {
            return name.ToLowerInvariant();
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

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