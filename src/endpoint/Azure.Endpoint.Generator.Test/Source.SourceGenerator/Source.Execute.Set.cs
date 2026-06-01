namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

internal static partial class FunctionSourceGeneratorSource
{
    internal const string EndpointSetSourceCode
        =
        """
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;
        using System.Collections.Generic;

        [assembly: EndpointFunctionSwagger]

        namespace Demo.Functions;

        [EndpointOperationMetadata("Products.Get", "GET", "/products/{id}")]
        [EndpointOperationMetadata("Products.Delete", "DELETE", "/products/{id}")]
        public sealed class ProductEndpointSet : IEndpointSet
        {
            public static IReadOnlyCollection<EndpointMetadata> Metadata { get; } = default!;
        }

        public static class FunctionProvider
        {
            [EndpointSetFunction]
            public static Dependency<ProductEndpointSet> UseProductEndpointSet()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            using System;

            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpointSet : IEndpointInvokeSupplier
            {
            }

            public sealed class EndpointMetadata
            {
                public string? OperationId { get; init; }
            }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public sealed class EndpointOperationMetadataAttribute : Attribute
            {
                public EndpointOperationMetadataAttribute(string operationId, string method, string route)
                {
                }
            }
        }

        namespace PrimeFuncPack
        {
            public sealed class Dependency<T>
            {
            }
        }
        """;

    internal const string ExecuteSet_EndpointSetFunction_GeneratesFunctionForEachOperation_ExpectedGetFunctionSource
        =
        """
        // Auto-generated code by PrimeFuncPack
        #nullable enable

        using GarageGroup.Infra.Endpoint;
        using Microsoft.Azure.Functions.Worker;
        using Microsoft.Azure.Functions.Worker.Http;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Demo.Functions;

        partial class FunctionProviderEndpointFunction
        {
            [Function("Products.Get")]
            public static Task<HttpResponseData> ProductsGetAsync(
                [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseProductEndpointSet()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal const string ExecuteSet_EndpointSetFunction_GeneratesFunctionForEachOperation_ExpectedDeleteFunctionSource
        =
        """
        // Auto-generated code by PrimeFuncPack
        #nullable enable

        using GarageGroup.Infra.Endpoint;
        using Microsoft.Azure.Functions.Worker;
        using Microsoft.Azure.Functions.Worker.Http;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Demo.Functions;

        partial class FunctionProviderEndpointFunction
        {
            [Function("Products.Delete")]
            public static Task<HttpResponseData> ProductsDeleteAsync(
                [HttpTrigger(AuthorizationLevel.Function, "DELETE", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseProductEndpointSet()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal const string ExecuteSet_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_SourceCode
        =
        """
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;
        using System;
        using System.Collections.Generic;

        namespace Demo.Functions;

        [EndpointOperationMetadata("Products.Get", "GET", "/products/{id}")]
        [EndpointOperationMetadata("Products.Delete", "DELETE", "/products/{id}")]
        public sealed class ProductEndpointSet : IEndpointSet
        {
            public static IReadOnlyCollection<EndpointMetadata> Metadata { get; } = default!;
        }

        [EndpointFunctionSecurity(FunctionAuthorizationLevel.Admin)]
        public static class FunctionProvider
        {
            [EndpointFunctionSecurity(FunctionAuthorizationLevel.User)]
            [EndpointSetFunction]
            public static Dependency<ProductEndpointSet> UseProductEndpointSet()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public sealed class EndpointOperationMetadataAttribute : Attribute
            {
                public EndpointOperationMetadataAttribute(string operationId, string method, string route)
                {
                }
            }

            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpointSet : IEndpointInvokeSupplier
            {
            }

            public sealed class EndpointMetadata
            {
                public string? OperationId { get; init; }
            }
        }

        namespace PrimeFuncPack
        {
            public sealed class Dependency<T>
            {
            }
        }
        """;

    internal const string ExecuteSet_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_ExpectedGetFunctionSource
        =
        """
        // Auto-generated code by PrimeFuncPack
        #nullable enable

        using GarageGroup.Infra.Endpoint;
        using Microsoft.Azure.Functions.Worker;
        using Microsoft.Azure.Functions.Worker.Http;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Demo.Functions;

        partial class FunctionProviderEndpointFunction
        {
            [Function("Products.Get")]
            public static Task<HttpResponseData> ProductsGetAsync(
                [HttpTrigger(AuthorizationLevel.User, "GET", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseProductEndpointSet()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal const string ExecuteSet_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_ExpectedDeleteFunctionSource
        =
        """
        // Auto-generated code by PrimeFuncPack
        #nullable enable

        using GarageGroup.Infra.Endpoint;
        using Microsoft.Azure.Functions.Worker;
        using Microsoft.Azure.Functions.Worker.Http;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Demo.Functions;

        partial class FunctionProviderEndpointFunction
        {
            [Function("Products.Delete")]
            public static Task<HttpResponseData> ProductsDeleteAsync(
                [HttpTrigger(AuthorizationLevel.User, "DELETE", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseProductEndpointSet()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal const string ExecuteSet_EndpointSetFunction_WithoutOperationMetadata_ThrowsInvalidOperationException_SourceCode
        =
        """
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;

        namespace Demo.Functions;

        public sealed class ProductEndpointSet : IEndpointSet
        {
        }

        public static class FunctionProvider
        {
            [EndpointSetFunction]
            public static Dependency<ProductEndpointSet> UseProductEndpointSet()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpointSet : IEndpointInvokeSupplier
            {
            }
        }

        namespace PrimeFuncPack
        {
            public sealed class Dependency<T>
            {
            }
        }
        """;
}