using System.Collections.Generic;
using System.Linq;
using PrimeFuncPack;

namespace GarageGroup.Infra;

internal static class SourceBuilderCompatibility
{
    internal static SourceBuilder AppendCodeLine(this SourceBuilder sourceBuilder, params string[] lines)
        =>
        sourceBuilder.AppendCodeLines(lines);

    internal static SourceBuilder AddUsings(this SourceBuilder sourceBuilder, IEnumerable<string> namespaces)
        =>
        sourceBuilder.AddUsing(namespaces.ToArray());
}
