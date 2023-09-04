using System.Collections.Generic;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
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