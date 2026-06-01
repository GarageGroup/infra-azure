namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

internal static partial class FunctionSourceGeneratorSource
{
    internal const string Execute_EndpointFunction_SwaggerGenerator_UsesEndpointMetadata_SourceCode
        =
        """
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;

        [assembly: EndpointFunctionSwagger]

        namespace Demo.Functions;

        [EndpointOperationMetadata("Products.Get", "GET", "/products/{id}")]
        public sealed class ProductEndpoint : IEndpoint
        {
        }

        public static class FunctionProvider
        {
            [EndpointFunction]
            public static Dependency<ProductEndpoint> UseProductEndpoint()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            using System;

            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpoint : IEndpointInvokeSupplier
            {
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

    internal const string Execute_EndpointFunction_SwaggerGenerator_UsesEndpointMetadata_ExpectedSource
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

        public static class SwaggerProviderSwagger
        {
            [Function("GetSwaggerDocument")]
            public static Task<HttpResponseData> GetSwaggerDocumentAsync(
                [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "swagger/swagger.{format}")] HttpRequestData request,
                string? format,
                CancellationToken cancellationToken)
                =>
                request.CreateStandardSwaggerBuilder()
                .AddFunctionEndpoint(ProductEndpoint.GetEndpointMetadata())
                .BuildResponseAsync(request, format, cancellationToken);
        }
        """;

    internal const string ExecuteSet_EndpointSetFunction_SwaggerGenerator_UsesEndpointSetMetadataPerOperation_SourceCode
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

        [EndpointMetadata("GET", "/health")]
        public sealed class HealthEndpoint : IEndpoint
        {
        }

        public static class FunctionProvider
        {
            [EndpointSetFunction]
            public static Dependency<ProductEndpointSet> UseProductEndpointSet()
                =>
                default!;

            [EndpointFunction("Health.Get")]
            public static Dependency<HealthEndpoint> UseHealthEndpoint()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            using System;

            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpoint : IEndpointInvokeSupplier
            {
            }

            public interface IEndpointSet : IEndpointInvokeSupplier
            {
            }

            public sealed class EndpointMetadata
            {
                public string? OperationId { get; init; }
            }

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class EndpointMetadataAttribute : Attribute
            {
                public EndpointMetadataAttribute(string method, string route)
                {
                }
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

    internal const string ExecuteSet_EndpointSetFunction_SwaggerGenerator_UsesEndpointSetMetadataPerOperation_ExpectedSource
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

        public static class SwaggerProviderSwagger
        {
            [Function("GetSwaggerDocument")]
            public static Task<HttpResponseData> GetSwaggerDocumentAsync(
                [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "swagger/swagger.{format}")] HttpRequestData request,
                string? format,
                CancellationToken cancellationToken)
                =>
                request.CreateStandardSwaggerBuilder()
                .AddFunctionEndpoints(ProductEndpointSet.Metadata)
                .AddFunctionEndpoint(HealthEndpoint.GetEndpointMetadata())
                .BuildResponseAsync(request, format, cancellationToken);
        }
        """;

    internal const string Execute_EndpointFunction_SwaggerGenerator_LevelSpecified_UsesSpecifiedAuthorizationLevel_SourceCode
        =
        """
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;

        [assembly: EndpointFunctionSwagger(FunctionAuthorizationLevel.System)]

        namespace Demo.Functions;

        [EndpointOperationMetadata("Products.Get", "GET", "/products/{id}")]
        public sealed class ProductEndpoint : IEndpoint
        {
        }

        public static class FunctionProvider
        {
            [EndpointFunction]
            public static Dependency<ProductEndpoint> UseProductEndpoint()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            using System;

            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpoint : IEndpointInvokeSupplier
            {
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

    internal const string Execute_EndpointFunction_SwaggerGenerator_LevelSpecified_UsesSpecifiedAuthorizationLevel_ExpectedSource
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

        public static class SwaggerProviderSwagger
        {
            [Function("GetSwaggerDocument")]
            public static Task<HttpResponseData> GetSwaggerDocumentAsync(
                [HttpTrigger(AuthorizationLevel.System, "GET", Route = "swagger/swagger.{format}")] HttpRequestData request,
                string? format,
                CancellationToken cancellationToken)
                =>
                request.CreateStandardSwaggerBuilder()
                .AddFunctionEndpoint(ProductEndpoint.GetEndpointMetadata())
                .BuildResponseAsync(request, format, cancellationToken);
        }
        """;

    internal const string Execute_EndpointFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointMetadata_SourceCode
        =
        """
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;

        [assembly: EndpointFunctionSwagger]

        namespace Demo.Functions;

        [EndpointOperationMetadata("Products.Get", "GET", "/products/{id}")]
        public sealed class ProductEndpoint : IEndpoint
        {
        }

        public static class FunctionProvider
        {
            [EndpointFunction(IsSwaggerHidden = true)]
            public static Dependency<ProductEndpoint> UseProductEndpoint()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            using System;

            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpoint : IEndpointInvokeSupplier
            {
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

    internal const string Execute_EndpointFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointMetadata_ExpectedSource
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

        public static class SwaggerProviderSwagger
        {
            [Function("GetSwaggerDocument")]
            public static Task<HttpResponseData> GetSwaggerDocumentAsync(
                [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "swagger/swagger.{format}")] HttpRequestData request,
                string? format,
                CancellationToken cancellationToken)
                =>
                request.CreateStandardSwaggerBuilder()
                .BuildResponseAsync(request, format, cancellationToken);
        }
        """;

    internal const string ExecuteSet_EndpointSetFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointSetMetadata_SourceCode
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

        [EndpointMetadata("GET", "/health")]
        public sealed class HealthEndpoint : IEndpoint
        {
        }

        public static class FunctionProvider
        {
            [EndpointSetFunction(IsSwaggerHidden = true)]
            public static Dependency<ProductEndpointSet> UseProductEndpointSet()
                =>
                default!;

            [EndpointFunction("Health.Get")]
            public static Dependency<HealthEndpoint> UseHealthEndpoint()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            using System;

            public interface IEndpointInvokeSupplier
            {
            }

            public interface IEndpoint : IEndpointInvokeSupplier
            {
            }

            public interface IEndpointSet : IEndpointInvokeSupplier
            {
            }

            public sealed class EndpointMetadata
            {
                public string? OperationId { get; init; }
            }

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class EndpointMetadataAttribute : Attribute
            {
                public EndpointMetadataAttribute(string method, string route)
                {
                }
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

    internal const string ExecuteSet_EndpointSetFunction_SwaggerGenerator_SwaggerHidden_DoesNotAddEndpointSetMetadata_ExpectedSource
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

        public static class SwaggerProviderSwagger
        {
            [Function("GetSwaggerDocument")]
            public static Task<HttpResponseData> GetSwaggerDocumentAsync(
                [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "swagger/swagger.{format}")] HttpRequestData request,
                string? format,
                CancellationToken cancellationToken)
                =>
                request.CreateStandardSwaggerBuilder()
                .AddFunctionEndpoint(HealthEndpoint.GetEndpointMetadata())
                .BuildResponseAsync(request, format, cancellationToken);
        }
        """;
}