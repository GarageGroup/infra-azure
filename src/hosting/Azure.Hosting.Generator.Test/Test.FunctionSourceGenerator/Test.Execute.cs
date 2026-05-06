using System.Linq;
using Xunit;

namespace GarageGroup.Infra.Azure.Hosting.Generator.Test;

partial class FunctionSourceGeneratorTest
{
    [Theory]
    [MemberData(nameof(SourceCodesDisabled))]
    public static void Execute_FunctionIsDisabled_DontGenerateSourceCode(
        string sourceCode)
    {
        var result = RunGenerator(sourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        Assert.Empty(generatorResult.GeneratedSources);
    }

    [Theory]
    [MemberData(nameof(SourceCodesNotDisabled))]
    public static void Execute_FunctionIsNotDisabled_GeneratesRefreshableTokenCredentialFunctionSource(
        string sourceCode, string expectedAttribute)
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
                $$"""
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using Microsoft.Azure.Functions.Worker;
                using System.Threading.Tasks;

                namespace GarageGroup.Infra;

                public static class RefreshableTokenCredentialFunction
                {
                    [Function("RefreshAzureTokens")]
                    [FixedDelayRetry(5, "00:00:10")]
                    public static Task RefreshAzureTokensAsync(
                        {{expectedAttribute}} object input, FunctionContext context)
                        =>
                        context.RefreshAzureTokensAsync();
                }
                """),
            source);
    }

    public static TheoryData<string> SourceCodesDisabled
        =>
        [
            """
            using GarageGroup.Infra;

            [assembly: RefreshableTokenCredential("0 */5 * * * *", IsDisabled = true)]

            namespace Some.Test;

            public static class SomeClass
            {
            }
            """,
            """
            [assembly: GarageGroup.Infra.RefreshableTokenCredential("", UseMonitor = true, RunOnStartup = true, IsDisabled = true)]
            namespace Some.Test;

            static class Program
            {
            }
            """
        ];

    public static TheoryData<string, string> SourceCodesNotDisabled
        =>
        [
            new(
                string.Empty,
                "[TimerTrigger(\"0 */50 * * * *\")]"),
            new(
                """
                namespace Some.Test;

                public static class Stub
                {
                }
                """,
                "[TimerTrigger(\"0 */50 * * * *\")]"),
            new(
                """
                using GarageGroup.Infra;

                [assembly: RefreshableTokenCredential("0 */5 * * * *", UseMonitor = true, RunOnStartup = true)]

                namespace Some.Test;

                public static class SomeClass
                {
                }
                """,
                "[TimerTrigger(\"0 */5 * * * *\", UseMonitor = true, RunOnStartup = true)]"),
            new(
                """
                using GarageGroup.Infra;

                [assembly: RefreshableTokenCredential("0 */5 * * * *", UseMonitor = false, RunOnStartup = false)]

                namespace Some.Test;

                public static class SomeClass
                {
                }
                """,
                "[TimerTrigger(\"0 */5 * * * *\")]"),
            new(
                """
                [assembly: GarageGroup.Infra.RefreshableTokenCredential(null!, UseMonitor = true)]
                namespace Some.Test;

                public static class Stub
                {
                }
                """,
                "[TimerTrigger(\"0 */50 * * * *\", UseMonitor = true)]"),
            new(
                """
                [assembly: GarageGroup.Infra.RefreshableTokenCredential("   ", RunOnStartup = true)]
                namespace Some.Test;

                static class Program
                {
                }
                """,
                "[TimerTrigger(\"0 */50 * * * *\", RunOnStartup = true)]")
        ];
}
