using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Azure.Handler.Generator.Test;

partial class HandlerFunctionSourceGeneratorTest
{
    [Theory]
    [MemberData(nameof(HttpFunctionAttributesCases))]
    public static void ExecuteHttp_ValidHttpResolver_GeneratesConstructorAndFunctionSources(
        string httpFunctionAttributeSourceCode,
        string expectedHttpTriggerSourceCode)
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
                [{{httpFunctionAttributeSourceCode}}]
                public static Dependency<IInputHandler> ResolveWork()
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
                $$"""
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
                using Microsoft.Azure.Functions.Worker.Http;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Demo.Functions;

                partial class FunctionProviderHandlerFunction
                {
                    [Function("DoWork")]
                    public static Task<HttpResponseData> DoWorkAsync(
                        [{{expectedHttpTriggerSourceCode}}] HttpRequestData requestData,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveWork()
                        .RunHttpFunctionAsync<IInputHandler, Input, Output>(
                            requestData, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsConstructor(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.g.cs", StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_ResolverIsNotStatic_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            using System;
            using GarageGroup.Infra;
            using PrimeFuncPack;

            namespace Demo.Invalid;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public sealed class InvalidProvider
            {
                [HttpFunction("DoWork")]
                public Dependency<IInputHandler> ResolveWork()
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("InvalidProvider.ResolveWork", exception.Message);
        Assert.Contains("must be static", exception.Message);
    }

    [Fact]
    public static void ExecuteHttp_ResolverReturnsCustomDependencyWithResolveMethod_GeneratesSources()
    {
        const string sourceCode =
            """
            using System;
            using GarageGroup.Infra;

            namespace Demo.Custom;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class CustomProvider
            {
                [HttpFunction("DoWork")]
                public static Some.Test.CustomDependency<IInputHandler> ResolveWork()
                    =>
                    default!;
            }

            namespace Some.Test
            {
                public sealed class CustomDependency<T>
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
        Assert.Equal(2, generatorResult.GeneratedSources.Length);
    }

    [Fact]
    public static void ExecuteHttp_ReadInputFuncSpecified_GeneratesRunHttpWithReadInputDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            internal static class FunctionProvider
            {
                [HttpFunction("DoWork", ReadInputFunc = nameof(ReadInput))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                internal static Result<Input?, Failure<HandlerFailureCode>> ReadInput(HttpRequestData requestData, string requestBody)
                    =>
                    default;
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
        var function = generatedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using GarageGroup.Infra;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Demo.Functions;

                partial class FunctionProviderHandlerFunction
                {
                    [Function("DoWork")]
                    public static Task<HttpResponseData> DoWorkAsync(
                        [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequestData requestData,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveWork()
                        .RunHttpFunctionAsync<IInputHandler, Input, Output>(
                            requestData, FunctionProvider.ReadInput, default, default, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_ReadInputFuncInvalidContract_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Invalid;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", ReadInputFunc = nameof(ReadInput))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                public static Result<Input, Failure<HandlerFailureCode>> ReadInput(HttpRequestData requestData, string requestBody)
                    =>
                    default;
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.ResolveWork", exception.Message);
        Assert.Contains("invalid ReadInputFunc", exception.Message);
    }

    [Fact]
    public static void ExecuteHttp_ReadInputFuncPrivate_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Invalid;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", ReadInputFunc = nameof(ReadInput))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                private static Result<Input?, Failure<HandlerFailureCode>> ReadInput(HttpRequestData requestData, string requestBody)
                    =>
                    default;
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.ResolveWork", exception.Message);
        Assert.Contains("invalid ReadInputFunc", exception.Message);
    }

    [Fact]
    public static void ExecuteHttp_ReadInputFuncTypeSpecified_GeneratesRunHttpWithExternalReadInputDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions
            {
                public sealed record class Input(int Id);

                public sealed record class Output(string Value);

                public interface IInputHandler : IHandler<Input, Output>;

                public static class FunctionProvider
                {
                    [HttpFunction(
                        "DoWork",
                        ReadInputFunc = nameof(Demo.Readers.HttpInputReader.ReadInput),
                        ReadInputFuncType = typeof(Demo.Readers.HttpInputReader))]
                    public static Dependency<IInputHandler> ResolveWork()
                        =>
                        default!;
                }
            }

            namespace Demo.Readers
            {
                using Demo.Functions;

                public static class HttpInputReader
                {
                    public static Result<Input?, Failure<HandlerFailureCode>> ReadInput(HttpRequestData requestData, string requestBody)
                        =>
                        default;
                }
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

        var function = generatorResult.GeneratedSources.Single(IsFunction).SourceText.ToString();
        Assert.Contains("using Demo.Readers;", function, StringComparison.Ordinal);
        Assert.Contains("HttpInputReader.ReadInput", function, StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_ReadInputFuncTypeSpecifiedInternalMethod_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Invalid
            {
                using Demo.Readers;

                public sealed record class Input(int Id);

                public sealed record class Output(string Value);

                public interface IInputHandler : IHandler<Input, Output>;

                public static class FunctionProvider
                {
                    [HttpFunction(
                        "DoWork",
                        ReadInputFunc = nameof(HttpInputReader.ReadInput),
                        ReadInputFuncType = typeof(HttpInputReader))]
                    public static Dependency<IInputHandler> ResolveWork()
                        =>
                        default!;
                }
            }

            namespace Demo.Readers
            {
                using Demo.Invalid;

                public static class HttpInputReader
                {
                    internal static Result<Input?, Failure<HandlerFailureCode>> ReadInput(HttpRequestData requestData, string requestBody)
                        =>
                        default;
                }
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.ResolveWork", exception.Message);
        Assert.Contains("invalid ReadInputFunc", exception.Message);
    }

    [Fact]
    public static void ExecuteHttp_CreateSuccessResponseFuncSpecified_GeneratesRunHttpWithCreateSuccessDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", CreateSuccessResponseFunc = nameof(CreateSuccessResponse))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                public static HttpResponseData CreateSuccessResponse(HttpRequestData requestData, Output output)
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
        var function = generatedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using GarageGroup.Infra;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Demo.Functions;

                partial class FunctionProviderHandlerFunction
                {
                    [Function("DoWork")]
                    public static Task<HttpResponseData> DoWorkAsync(
                        [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequestData requestData,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveWork()
                        .RunHttpFunctionAsync<IInputHandler, Input, Output>(
                            requestData, default, FunctionProvider.CreateSuccessResponse, default, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_CreateSuccessResponseFuncPrivate_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Invalid;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", CreateSuccessResponseFunc = nameof(CreateSuccessResponse))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                private static HttpResponseData CreateSuccessResponse(HttpRequestData requestData, Output output)
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.ResolveWork", exception.Message);
        Assert.Contains("invalid CreateSuccessResponseFunc", exception.Message);
    }

    [Fact]
    public static void ExecuteHttp_CreateSuccessResponseFuncInternalInProvider_GeneratesRunHttpWithCreateSuccessDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", CreateSuccessResponseFunc = nameof(CreateSuccessResponse))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                internal static HttpResponseData CreateSuccessResponse(HttpRequestData requestData, Output output)
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

        var function = generatorResult.GeneratedSources.Single(IsFunction).SourceText.ToString();
        Assert.Contains("FunctionProvider.CreateSuccessResponse", function, StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_CreateSuccessResponseFuncTypeSpecified_GeneratesRunHttpWithExternalCreateSuccessDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions
            {
                using Demo.HttpResponses;

                public sealed record class Input(int Id);

                public sealed record class Output(string Value);

                public interface IInputHandler : IHandler<Input, Output>;

                public static class FunctionProvider
                {
                    [HttpFunction(
                        "DoWork",
                        CreateSuccessResponseFunc = nameof(HttpResponseFactory.CreateSuccessResponse),
                        CreateSuccessResponseFuncType = typeof(HttpResponseFactory))]
                    public static Dependency<IInputHandler> ResolveWork()
                        =>
                        default!;
                }
            }

            namespace Demo.HttpResponses
            {
                using Demo.Functions;

                public static class HttpResponseFactory
                {
                    public static HttpResponseData CreateSuccessResponse(HttpRequestData requestData, Output output)
                        =>
                        default!;
                }
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

        var function = generatorResult.GeneratedSources.Single(IsFunction).SourceText.ToString();
        Assert.Contains("using Demo.HttpResponses;", function, StringComparison.Ordinal);
        Assert.Contains("HttpResponseFactory.CreateSuccessResponse", function, StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_CreateSuccessResponseFuncTypeSpecifiedInternalMethod_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Invalid
            {
                using Demo.HttpResponses;

                public sealed record class Input(int Id);

                public sealed record class Output(string Value);

                public interface IInputHandler : IHandler<Input, Output>;

                public static class FunctionProvider
                {
                    [HttpFunction(
                        "DoWork",
                        CreateSuccessResponseFunc = nameof(HttpResponseFactory.CreateSuccessResponse),
                        CreateSuccessResponseFuncType = typeof(HttpResponseFactory))]
                    public static Dependency<IInputHandler> ResolveWork()
                        =>
                        default!;
                }
            }

            namespace Demo.HttpResponses
            {
                using Demo.Invalid;

                public static class HttpResponseFactory
                {
                    internal static HttpResponseData CreateSuccessResponse(HttpRequestData requestData, Output output)
                        =>
                        default!;
                }
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.ResolveWork", exception.Message);
        Assert.Contains("invalid CreateSuccessResponseFunc", exception.Message);
    }

    [Fact]
    public static void ExecuteHttp_ReadInputAndCreateSuccessSpecified_GeneratesRunHttpWithoutGenericArguments()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", ReadInputFunc = nameof(ReadInput), CreateSuccessResponseFunc = nameof(CreateSuccessResponse))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                public static Result<Input?, Failure<HandlerFailureCode>> ReadInput(HttpRequestData requestData, string requestBody)
                    =>
                    default;

                public static HttpResponseData CreateSuccessResponse(HttpRequestData requestData, Output output)
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

        var function = generatorResult.GeneratedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using GarageGroup.Infra;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Demo.Functions;

                partial class FunctionProviderHandlerFunction
                {
                    [Function("DoWork")]
                    public static Task<HttpResponseData> DoWorkAsync(
                        [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequestData requestData,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveWork()
                        .RunHttpFunctionAsync(
                            requestData, FunctionProvider.ReadInput, FunctionProvider.CreateSuccessResponse, default, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_CreateFailureResponseFuncSpecified_GeneratesRunHttpWithCreateFailureDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", CreateFailureResponseFunc = nameof(CreateFailureResponse))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                public static HttpResponseData CreateFailureResponse(HttpRequestData requestData, Failure<HandlerFailureCode> failure)
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

        var function = generatorResult.GeneratedSources.Single(IsFunction).SourceText.ToString();

        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using GarageGroup.Infra;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Demo.Functions;

                partial class FunctionProviderHandlerFunction
                {
                    [Function("DoWork")]
                    public static Task<HttpResponseData> DoWorkAsync(
                        [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequestData requestData,
                        CancellationToken cancellationToken)
                        =>
                        FunctionProvider.ResolveWork()
                        .RunHttpFunctionAsync<IInputHandler, Input, Output>(
                            requestData, default, default, FunctionProvider.CreateFailureResponse, cancellationToken);
                }
                """),
            NormalizeNewLines(function));

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_CreateFailureResponseFuncPrivate_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Invalid;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", CreateFailureResponseFunc = nameof(CreateFailureResponse))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                private static HttpResponseData CreateFailureResponse(HttpRequestData requestData, Failure<HandlerFailureCode> failure)
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.ResolveWork", exception.Message);
        Assert.Contains("invalid CreateFailureResponseFunc", exception.Message);
    }

    [Fact]
    public static void ExecuteHttp_CreateFailureResponseFuncInternalInProvider_GeneratesRunHttpWithCreateFailureDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions;

            public sealed record class Input(int Id);

            public sealed record class Output(string Value);

            public interface IInputHandler : IHandler<Input, Output>;

            public static class FunctionProvider
            {
                [HttpFunction("DoWork", CreateFailureResponseFunc = nameof(CreateFailureResponse))]
                public static Dependency<IInputHandler> ResolveWork()
                    =>
                    default!;

                internal static HttpResponseData CreateFailureResponse(HttpRequestData requestData, Failure<HandlerFailureCode> failure)
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

        var function = generatorResult.GeneratedSources.Single(IsFunction).SourceText.ToString();
        Assert.Contains("FunctionProvider.CreateFailureResponse", function, StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_CreateFailureResponseFuncTypeSpecified_GeneratesRunHttpWithExternalCreateFailureDelegate()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Functions
            {
                using Demo.HttpResponses;

                public sealed record class Input(int Id);

                public sealed record class Output(string Value);

                public interface IInputHandler : IHandler<Input, Output>;

                public static class FunctionProvider
                {
                    [HttpFunction(
                        "DoWork",
                        CreateFailureResponseFunc = nameof(HttpResponseFactory.CreateFailureResponse),
                        CreateFailureResponseFuncType = typeof(HttpResponseFactory))]
                    public static Dependency<IInputHandler> ResolveWork()
                        =>
                        default!;
                }
            }

            namespace Demo.HttpResponses
            {
                public static class HttpResponseFactory
                {
                    public static HttpResponseData CreateFailureResponse(HttpRequestData requestData, Failure<HandlerFailureCode> failure)
                        =>
                        default!;
                }
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

        var function = generatorResult.GeneratedSources.Single(IsFunction).SourceText.ToString();
        Assert.Contains("using Demo.HttpResponses;", function, StringComparison.Ordinal);
        Assert.Contains("HttpResponseFactory.CreateFailureResponse", function, StringComparison.Ordinal);

        static bool IsFunction(GeneratedSourceResult source)
            =>
            source.HintName.Equals("FunctionProviderHandlerFunction.DoWorkAsync.g.cs", StringComparison.Ordinal);
    }

    [Fact]
    public static void ExecuteHttp_CreateFailureResponseFuncTypeSpecifiedInternalMethod_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            #nullable enable

            using System;
            using GarageGroup.Infra;
            using Microsoft.Azure.Functions.Worker.Http;
            using PrimeFuncPack;

            namespace Demo.Invalid
            {
                using Demo.HttpResponses;

                public sealed record class Input(int Id);

                public sealed record class Output(string Value);

                public interface IInputHandler : IHandler<Input, Output>;

                public static class FunctionProvider
                {
                    [HttpFunction(
                        "DoWork",
                        CreateFailureResponseFunc = nameof(HttpResponseFactory.CreateFailureResponse),
                        CreateFailureResponseFuncType = typeof(HttpResponseFactory))]
                    public static Dependency<IInputHandler> ResolveWork()
                        =>
                        default!;
                }
            }

            namespace Demo.HttpResponses
            {
                public static class HttpResponseFactory
                {
                    internal static HttpResponseData CreateFailureResponse(HttpRequestData requestData, Failure<HandlerFailureCode> failure)
                        =>
                        default!;
                }
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
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("FunctionProvider.ResolveWork", exception.Message);
        Assert.Contains("invalid CreateFailureResponseFunc", exception.Message);
    }

    public static TheoryData<string, string> HttpFunctionAttributesCases
        =>
        new()
        {
            {
                "HttpFunction(\"DoWork\", HttpMethodName.Put, Route = \"v1/work\", AuthLevel = HttpAuthorizationLevel.Anonymous)",
                "HttpTrigger(AuthorizationLevel.Anonymous, \"PUT\", Route = \"v1/work\")"
            },
            {
                "HttpFunction(\"DoWork\", HttpMethodName.Get, Route = \"v2/items\", AuthLevel = HttpAuthorizationLevel.User)",
                "HttpTrigger(AuthorizationLevel.User, \"GET\", Route = \"v2/items\")"
            },
            {
                "HttpFunction(\"DoWork\", HttpMethodName.Delete, AuthLevel = HttpAuthorizationLevel.Function)",
                "HttpTrigger(AuthorizationLevel.Function, \"DELETE\")"
            },
            {
                "HttpFunction(\"DoWork\", HttpMethodName.Trace, Route = \"trace/{id}\", AuthLevel = HttpAuthorizationLevel.System)",
                "HttpTrigger(AuthorizationLevel.System, \"TRACE\", Route = \"trace/{id}\")"
            },
            {
                "HttpFunction(\"DoWork\", HttpMethodName.Patch, AuthLevel = HttpAuthorizationLevel.Admin)",
                "HttpTrigger(AuthorizationLevel.Admin, \"PATCH\")"
            },
            {
                "HttpFunction(\"DoWork\", HttpMethodName.Options, AuthLevel = (HttpAuthorizationLevel)17)",
                "HttpTrigger((AuthorizationLevel)17, \"OPTIONS\")"
            },
            {
                "HttpFunction(\"DoWork\", Route = \"default/post\")",
                "HttpTrigger(AuthorizationLevel.Anonymous, \"POST\", Route = \"default/post\")"
            }
        };
}
