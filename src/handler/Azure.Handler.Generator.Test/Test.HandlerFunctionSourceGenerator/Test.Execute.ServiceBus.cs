using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Azure.Handler.Generator.Test;

partial class HandlerFunctionSourceGeneratorTest
{
    [Theory]
    [MemberData(nameof(ServiceBusTriggerCases))]
    public static void ExecuteServiceBus_ValidResolver_GeneratesConstructorAndFunctionSources(
        string serviceBusFunctionAttribute,
        string expectedTriggerAttribute)
    {
        var sourceCode =
            $$"""
            using System;
            using GarageGroup.Infra;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [{{serviceBusFunctionAttribute}}]
                public static Dependency<IInputHandler> ResolveHandle()
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
                $$"""
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
                    [Function("Handle")]
                    public static Task HandleAsync(
                        [{{expectedTriggerAttribute}}] JsonElement requestData,
                        FunctionContext context,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveHandle()
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
            source.HintName.Equals("FunctionProviderHandlerFunction.HandleAsync.g.cs", StringComparison.Ordinal);
    }

    public static TheoryData<string, string> ServiceBusTriggerCases
        =>
        new()
        {
            {
                "ServiceBusFunction(\"Handle\", \"orders\", \"ServiceBus\")",
                "ServiceBusTrigger(\"orders\", Connection = \"ServiceBus\")"
            },
            {
                "ServiceBusFunction(\"Handle\", \"orders-topic\", \"orders-subscription\", \"ServiceBus\")",
                "ServiceBusTrigger(\"orders-topic\", \"orders-subscription\", Connection = \"ServiceBus\")"
            }
        };
}
