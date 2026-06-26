using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Azure.DurableTask.Generator.Test;

partial class DurableHandlerFunctionSourceGeneratorTest
{
    [Fact]
    public static void ExecuteOrchestration_ValidResolver_GeneratesConstructorAndFunctionSources()
    {
        const string sourceCode =
            """
            using System;
            using GarageGroup.Infra;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IOrchestrationHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [OrchestrationFunction("RunOrchestration")]
                public static Dependency<IOrchestrationHandler> ResolveOrchestration()
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
                using Microsoft.DurableTask;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Demo.Functions;

                partial class FunctionProviderDurableHandlerFunction
                {
                    [Function("RunOrchestration")]
                    public static Task<Output> RunOrchestrationAsync(
                        [OrchestrationTrigger] TaskOrchestrationContext orchestrationContext,
                        FunctionContext functionContext,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveOrchestration()
                        .RunOrchestrationFunctionAsync<IOrchestrationHandler, Input, Output>(
                            orchestrationContext, functionContext, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsConstructor(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderDurableHandlerFunction.g.cs", StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderDurableHandlerFunction.RunOrchestrationAsync.g.cs", StringComparison.Ordinal);
    }
}
