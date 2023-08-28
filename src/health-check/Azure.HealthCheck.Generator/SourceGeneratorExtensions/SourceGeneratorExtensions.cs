using System.Collections.Generic;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string DefaultNamespace = "GarageGroup.Infra";

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