using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Azure.Handler.Generator.Test;

partial class HandlerFunctionSourceGeneratorTest
{
    [Fact]
    public static void ExecuteEventGrid_ValidResolver_GeneratesConstructorAndFunctionSources()
    {
        const string sourceCode =
            """
            using System;
            using GarageGroup.Infra;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [EventGridFunction("OnEvent")]
                public static Dependency<IInputHandler> ResolveEvent()
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

                public static partial class FunctionProviderHandlerFunction
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

                partial class FunctionProviderHandlerFunction
                {
                    [Function("OnEvent")]
                    public static Task OnEventAsync(
                        [EventGridTrigger] JsonElement requestData,
                        FunctionContext context,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveEvent()
                        .RunAzureFunctionAsync<IInputHandler, Input, Output>(
                            requestData, context, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsConstructor(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.g.cs", StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.OnEventAsync.g.cs", StringComparison.Ordinal);
    }
}
