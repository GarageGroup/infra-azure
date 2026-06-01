namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

internal static partial class FunctionSourceGeneratorSource
{
    internal static readonly string Execute_NameSpecifiedAndOperationMetadataExists_SourceCode
        =
        BuildEndpointSourceCode(
            endpointAttributeSourceCode:
            "[EndpointOperationMetadata(\"Products.Get\", \"GET\", \"/products/{id}\")][EndpointMetadata(\"POST\", \"/legacy-route\")]",
            endpointFunctionAttributeSourceCode:
            "[EndpointFunction(\"Legacy.Products.Get\")]",
            endpointMetadataDeclarationSourceCode:
            "[AttributeUsage(AttributeTargets.Class)] public sealed class EndpointMetadataAttribute " +
            ": Attribute { public EndpointMetadataAttribute(string method, string route) { } }\n" +
            "[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)] public sealed class EndpointOperationMetadataAttribute " +
            ": Attribute { public EndpointOperationMetadataAttribute(string operationId, string method, string route) { } }",
            endpointMethodName:
            "UseGetProductEndpoint");

    internal const string Execute_NameSpecifiedAndOperationMetadataExists_ExpectedFunctionSource
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
            public static Task<HttpResponseData> ProductGetAsync(
                [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseGetProductEndpoint()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal static readonly string Execute_NameNotSpecifiedAndOperationMetadataExists_SourceCode
        =
        BuildEndpointSourceCode(
            endpointAttributeSourceCode:
            "[EndpointOperationMetadata(\"Products.Delete\", \"DELETE\", \"/products/{id}\")]",
            endpointFunctionAttributeSourceCode:
            "[EndpointFunction]",
            endpointMetadataDeclarationSourceCode:
            "[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)] public sealed class EndpointOperationMetadataAttribute " +
            ": Attribute { public EndpointOperationMetadataAttribute(string operationId, string method, string route) { } }",
            endpointMethodName:
            "UseDeleteProductEndpoint");

    internal const string Execute_NameNotSpecifiedAndOperationMetadataExists_ExpectedFunctionSource
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
            public static Task<HttpResponseData> ProductDeleteAsync(
                [HttpTrigger(AuthorizationLevel.Function, "DELETE", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseDeleteProductEndpoint()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal static readonly string Execute_NameSpecifiedAndOnlyLegacyEndpointMetadataExists_SourceCode
        =
        BuildEndpointSourceCode(
            endpointAttributeSourceCode:
            "[EndpointMetadata(\"PUT\", \"/products/{id}\")]",
            endpointFunctionAttributeSourceCode:
            "[EndpointFunction(\"Products.Update.Legacy\")]",
            endpointMetadataDeclarationSourceCode:
            "[AttributeUsage(AttributeTargets.Class)] public sealed class EndpointMetadataAttribute " +
            ": Attribute { public EndpointMetadataAttribute(string method, string route) { } }",
            endpointMethodName:
            "UseUpdateProductEndpoint");

    internal const string Execute_NameSpecifiedAndOnlyLegacyEndpointMetadataExists_ExpectedFunctionSource
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
            [Function("Products.Update.Legacy")]
            public static Task<HttpResponseData> ProductUpdateAsync(
                [HttpTrigger(AuthorizationLevel.Function, "PUT", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseUpdateProductEndpoint()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal const string Execute_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_SourceCode
        =
        """
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;
        using System;

        namespace Demo.Functions;

        [EndpointOperationMetadata("Products.Get", "GET", "/products/{id}")]
        public sealed class ProductEndpoint : IEndpoint
        {
        }

        [EndpointFunctionSecurity(FunctionAuthorizationLevel.Admin)]
        public static class FunctionProvider
        {
            [EndpointFunctionSecurity(FunctionAuthorizationLevel.User)]
            [EndpointFunction]
            public static Dependency<ProductEndpoint> UseGetProductEndpoint()
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

            public interface IEndpoint
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

    internal const string Execute_EndpointFunctionSecurityAttribute_MethodOverridesTypeAuthorizationLevel_ExpectedFunctionSource
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
            public static Task<HttpResponseData> ProductGetAsync(
                [HttpTrigger(AuthorizationLevel.User, "GET", Route = "products/{id}")] HttpRequestData requestData,
                CancellationToken cancellationToken)
                =>
                FunctionProvider.UseGetProductEndpoint()
                .RunAzureFunctionAsync(
                    requestData, cancellationToken);
        }
        """;

    internal static readonly string Execute_NameNotSpecifiedAndOperationMetadataDoesNotExist_SourceCode
        =
        BuildEndpointSourceCode(
            endpointAttributeSourceCode:
            "[EndpointMetadata(\"GET\", \"/products/{id}\")]",
            endpointFunctionAttributeSourceCode:
            "[EndpointFunction]",
            endpointMetadataDeclarationSourceCode:
            "[AttributeUsage(AttributeTargets.Class)] public sealed class EndpointMetadataAttribute " +
            ": Attribute { public EndpointMetadataAttribute(string method, string route) { } }",
            endpointMethodName:
            "UseGetProductEndpoint");
}