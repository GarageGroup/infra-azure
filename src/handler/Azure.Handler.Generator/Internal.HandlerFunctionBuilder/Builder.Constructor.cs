using System.Linq;
using System.Text;

namespace GarageGroup.Infra;

partial class HandlerFunctionBuilder
{
    internal static string BuildConstructorSourceCode(this FunctionProviderMetadata provider)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AppendCodeLine(
            provider.BuildClassHeader())
        .BeginCodeBlock()
        .EndCodeBlock()
        .Build();

    private static string BuildClassHeader(this FunctionProviderMetadata provider)
    {
        var builder = new StringBuilder("public static");

        if (provider.ResolverTypes.Any())
        {
            builder = builder.Append(" partial");
        }

        return builder.Append(" class ").Append(provider.TypeName).ToString();
    }
}