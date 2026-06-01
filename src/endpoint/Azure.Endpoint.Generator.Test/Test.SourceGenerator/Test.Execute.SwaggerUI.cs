using System.Linq;
using Xunit;

namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

using static FunctionSourceGeneratorSource;

partial class FunctionSourceGeneratorTest
{
    [Fact]
    public static void ExecuteSwaggerUI_AssemblyAttribute_GeneratesSwaggerUIFunction()
    {
        var result = RunSwaggerUIGenerator(ExecuteSwaggerUI_AssemblyAttribute_GeneratesSwaggerUIFunction_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwaggerUI.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(ExecuteSwaggerUI_AssemblyAttribute_GeneratesSwaggerUIFunction_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }

    [Fact]
    public static void ExecuteSwaggerUI_LevelSpecified_UsesSpecifiedAuthorizationLevel()
    {
        var result = RunSwaggerUIGenerator(ExecuteSwaggerUI_LevelSpecified_UsesSpecifiedAuthorizationLevel_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwaggerUI.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(ExecuteSwaggerUI_LevelSpecified_UsesSpecifiedAuthorizationLevel_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }
}
