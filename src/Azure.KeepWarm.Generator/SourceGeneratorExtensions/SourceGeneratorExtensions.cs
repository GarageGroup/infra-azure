using System.Collections.Generic;

namespace GGroupp.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string DefaultNamespace = "GGroupp.Infra";

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
}