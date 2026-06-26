using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Azure.DurableTask.Generator.Test;

partial class DurableHandlerFunctionSourceGeneratorTest
{
    [Fact]
    public static void ExecuteActivity_ValidResolver_GeneratesConstructorAndFunctionSources()
    {
        const string sourceCode =
            """
            using System;
            using GarageGroup.Infra;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IActivityHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [ActivityFunction("RunActivity")]
                public static Dependency<IActivityHandler> ResolveActivity()
                    =>
                    default!;
            }

            namespace PrimeFuncPack
            {
                public sealed class Dependency<T>
                {
                    public T Resolve(IServiceProvider serviceProvider)
                        =>
                        default!;
                }
            }
            """;

        var result = RunGenerator(sourceCode);
        var generatorResult = result.Results.Single();

        Assert.Null(generatorResult.Exception);
        Assert.Empty(result.Diagnostics);

        var generatedSources = generatorResult.GeneratedSources;
        Assert.Equal(2, generatedSources.Length);

        var constructor = generatedSources.Single(IsConstructor).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                namespace Demo.Functions;

                public static partial class FunctionProviderDurableHandlerFunction
                {
                }
                """),
            NormalizeNewLines(constructor));

        var function = generatedSources.Single(IsFunction).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using GarageGroup.Infra;
                using Microsoft.Azure.Functions.Worker;
                using System.Text.Json;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Demo.Functions;

                partial class FunctionProviderDurableHandlerFunction
                {
                    [Function("RunActivity")]
                    public static Task<HandlerResultJson<Output>> RunActivityAsync(
                        [ActivityTrigger] JsonElement requestData,
                        FunctionContext context,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveActivity()
                        .RunAzureFunctionAsync<IActivityHandler, Input, Output>(
                            requestData, context, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsConstructor(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderDurableHandlerFunction.g.cs", StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderDurableHandlerFunction.RunActivityAsync.g.cs", StringComparison.Ordinal);
    }
}