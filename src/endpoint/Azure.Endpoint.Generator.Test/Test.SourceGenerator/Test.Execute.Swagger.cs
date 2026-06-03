using System.Linq;
using Xunit;

namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

using static FunctionSourceGeneratorSource;

partial class FunctionSourceGeneratorTest
{
    [Fact]
    public static void Execute_EndpointFunction_SwaggerGenerator_UsesEndpointMetadata()
    {
        var result = RunSwaggerGenerator(Execute_EndpointFunction_SwaggerGenerator_UsesEndpointMetadata_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwagger.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(Execute_EndpointFunction_SwaggerGenerator_UsesEndpointMetadata_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }

    [Fact]
    public static void ExecuteSet_EndpointSetFunction_SwaggerGenerator_UsesEndpointSetMetadataPerOperation()
    {
        var result = RunSwaggerGenerator(ExecuteSet_EndpointSetFunction_SwaggerGenerator_UsesEndpointSetMetadataPerOperation_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwagger.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(ExecuteSet_EndpointSetFunction_SwaggerGenerator_UsesEndpointSetMetadataPerOperation_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }

    [Fact]
    public static void Execute_EndpointFunction_SwaggerGenerator_LevelSpecified_UsesSpecifiedAuthorizationLevel()
    {
        var result = RunSwaggerGenerator(Execute_EndpointFunction_SwaggerGenerator_LevelSpecified_UsesSpecifiedAuthorizationLevel_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwagger.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(Execute_EndpointFunction_SwaggerGenerator_LevelSpecified_UsesSpecifiedAuthorizationLevel_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }

    [Fact]
    public static void Execute_EndpointFunction_SwaggerGenerator_AnonymousFunction_DoesNotRequireAuthorization()
    {
        var result = RunSwaggerGenerator(Execute_EndpointFunction_SwaggerGenerator_AnonymousFunction_DoesNotRequireAuthorization_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwagger.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(Execute_EndpointFunction_SwaggerGenerator_AnonymousFunction_DoesNotRequireAuthorization_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }

    [Fact]
    public static void Execute_EndpointFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointMetadata()
    {
        var result = RunSwaggerGenerator(Execute_EndpointFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointMetadata_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwagger.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(Execute_EndpointFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointMetadata_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }

    [Fact]
    public static void ExecuteSet_EndpointSetFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointSetMetadata()
    {
        var result = RunSwaggerGenerator(ExecuteSet_EndpointSetFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointSetMetadata_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSource = generatorResult.GeneratedSources.Single();
        Assert.Equal("SwaggerProviderSwagger.g.cs", generatedSource.HintName);

        Assert.Equal(
            NormalizeNewLines(ExecuteSet_EndpointSetFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointSetMetadata_ExpectedSource),
            NormalizeNewLines(generatedSource.SourceText.ToString()));
    }
}