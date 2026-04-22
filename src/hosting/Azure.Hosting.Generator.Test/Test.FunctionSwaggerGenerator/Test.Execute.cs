using System.Linq;
using Xunit;

namespace GarageGroup.Infra.Azure.Hosting.Generator.Test;

partial class FunctionSwaggerGeneratorTest
{
    [Theory]
    [MemberData(nameof(SourceCodes))]
    public static void Execute_GeneratesRefreshableTokenCredentialFunctionSource(string sourceCode)
    {
        var result = RunGenerator(sourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();

        Assert.Equal("RefreshableTokenCredentialFunction.g.cs", generatedSource.HintName);

        var source = NormalizeNewLines(generatedSource.SourceText.ToString());
        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using Microsoft.Azure.Functions.Worker;
                using System.Threading.Tasks;

                namespace GarageGroup.Infra;

                public static class RefreshableTokenCredentialFunction
                {
                    [Function("RefreshAzureTokens")]
                    [FixedDelayRetry(5, "00:00:10")]
                    public static Task RefreshAzureTokensAsync([TimerTrigger("0 */30 * * * *")] object input, FunctionContext context)
                        =>
                        context.RefreshAzureTokensAsync();
                }
                """),
            source);
    }
}
