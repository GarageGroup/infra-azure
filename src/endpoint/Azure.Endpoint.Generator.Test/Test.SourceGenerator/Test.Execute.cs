using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

using static FunctionSourceGeneratorSource;

partial class FunctionSourceGeneratorTest
{
    [Fact]
    public static void Execute_NameSpecifiedAndOperationMetadataExists_UsesOperationIdFromOperationMetadata()
    {
        var result = RunGenerator(Execute_NameSpecifiedAndOperationMetadataExists_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSources = generatorResult.GeneratedSources;
        Assert.Equal(2, generatedSources.Length);

        var functionSource = generatedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(Execute_NameSpecifiedAndOperationMetadataExists_ExpectedFunctionSource),
            NormalizeNewLines(functionSource));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductGetAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void Execute_NameNotSpecifiedAndOperationMetadataExists_UsesOperationIdFromOperationMetadata()
    {
        var result = RunGenerator(Execute_NameNotSpecifiedAndOperationMetadataExists_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSources = generatorResult.GeneratedSources;
        Assert.Equal(2, generatedSources.Length);

        var functionSource = generatedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(Execute_NameNotSpecifiedAndOperationMetadataExists_ExpectedFunctionSource),
            NormalizeNewLines(functionSource));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductDeleteAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void Execute_NameSpecifiedAndOnlyLegacyEndpointMetadataExists_UsesNameFromEndpointFunctionAttribute()
    {
        var result = RunGenerator(Execute_NameSpecifiedAndOnlyLegacyEndpointMetadataExists_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSources = generatorResult.GeneratedSources;
        Assert.Equal(2, generatedSources.Length);

        var functionSource = generatedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(Execute_NameSpecifiedAndOnlyLegacyEndpointMetadataExists_ExpectedFunctionSource),
            NormalizeNewLines(functionSource));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductUpdateAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void Execute_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel()
    {
        var result = RunGenerator(Execute_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSources = generatorResult.GeneratedSources;
        Assert.Equal(2, generatedSources.Length);

        var functionSource = generatedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(Execute_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_ExpectedFunctionSource),
            NormalizeNewLines(functionSource));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductGetAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void Execute_NameNotSpecifiedAndOperationMetadataDoesNotExist_ThrowsInvalidOperationException()
    {
        var result = RunGenerator(Execute_NameNotSpecifiedAndOperationMetadataDoesNotExist_SourceCode);
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.UseGetProductEndpoint", exception.Message);
        Assert.Contains("EndpointOperationMetadataAttribute", exception.Message);
    }

    [Fact]
    public static void EndpointFunctionAttribute_NameConstructor_Obsolete()
    {
        var constructor = typeof(EndpointFunctionAttribute).GetConstructor([typeof(string)]);
        Assert.NotNull(constructor);

        var obsoleteAttribute = Assert.IsType<ObsoleteAttribute>(Attribute.GetCustomAttribute(constructor!, typeof(ObsoleteAttribute)));

        Assert.Contains("obsolete", obsoleteAttribute.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EndpointOperationMetadataAttribute", obsoleteAttribute.Message, StringComparison.Ordinal);
    }
}