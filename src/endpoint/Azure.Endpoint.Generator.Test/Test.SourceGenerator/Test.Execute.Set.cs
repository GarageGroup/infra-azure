using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

using static FunctionSourceGeneratorSource;

partial class FunctionSourceGeneratorTest
{
    [Fact]
    public static void ExecuteSet_EndpointSetFunction_GeneratesFunctionForEachOperation()
    {
        var result = RunGenerator(EndpointSetSourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSources = generatorResult.GeneratedSources;
        Assert.Equal(3, generatedSources.Length);

        var getFunctionSource = generatedSources.Single(IsGetFunction).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(ExecuteSet_EndpointSetFunction_GeneratesFunctionForEachOperation_ExpectedGetFunctionSource),
            NormalizeNewLines(getFunctionSource));

        var deleteFunctionSource = generatedSources.Single(IsDeleteFunction).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(ExecuteSet_EndpointSetFunction_GeneratesFunctionForEachOperation_ExpectedDeleteFunctionSource),
            NormalizeNewLines(deleteFunctionSource));

        static bool IsGetFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductsGetAsync.g.cs", StringComparison.Ordinal);

        static bool IsDeleteFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductsDeleteAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteSet_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel()
    {
        var result = RunGenerator(ExecuteSet_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_SourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSources = generatorResult.GeneratedSources;
        Assert.Equal(3, generatedSources.Length);

        var getFunctionSource = generatedSources.Single(IsGetFunction).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(
                ExecuteSet_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_ExpectedGetFunctionSource),
            NormalizeNewLines(getFunctionSource));

        var deleteFunctionSource = generatedSources.Single(IsDeleteFunction).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(
                ExecuteSet_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_ExpectedDeleteFunctionSource),
            NormalizeNewLines(deleteFunctionSource));

        static bool IsGetFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductsGetAsync.g.cs", StringComparison.Ordinal);

        static bool IsDeleteFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderEndpointFunction.ProductsDeleteAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteSet_EndpointSetFunction_WithoutOperationMetadata_ThrowsInvalidOperationException()
    {
        var result = RunGenerator(ExecuteSet_EndpointSetFunction_WithoutOperationMetadata_ThrowsInvalidOperationException_SourceCode);
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.UseProductEndpointSet", exception.Message, StringComparison.Ordinal);
        Assert.Contains("EndpointOperationMetadataAttribute", exception.Message, StringComparison.Ordinal);
    }
}