using System.Linq;
using System.Text;

namespace GarageGroup.Infra;

partial class FunctionBuilder
{
    internal static string BuildConstructorSourceCode(this FunctionProviderMetadata provider)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AppendTypeHeader(
            provider)
        .BeginCodeBlock()
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendTypeHeader(this SourceBuilder sourceBuilder, FunctionProviderMetadata provider)
    {
        var headerBuilder = new StringBuilder("public static ");

        if (provider.ResolverTypes.Any())
        {
            headerBuilder = headerBuilder.Append("partial ");
        }

        headerBuilder = headerBuilder.Append("class ").Append(provider.TypeName);
        return sourceBuilder.AppendCodeLine(headerBuilder.ToString());
    }
}