namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

internal static partial class FunctionSourceGeneratorSource
{
    private static string BuildEndpointSourceCode(
        string endpointAttributeSourceCode,
        string endpointFunctionAttributeSourceCode,
        string endpointMetadataDeclarationSourceCode,
        string endpointMethodName)
        =>
        $$"""
        using GarageGroup.Infra;
        using GarageGroup.Infra.Endpoint;
        using PrimeFuncPack;

        namespace Demo.Functions;

        public interface IProductEndpoint : IEndpoint
        {
        }

        {{endpointAttributeSourceCode}}
        public sealed class ProductEndpoint : IProductEndpoint
        {
        }

        public static class FunctionProvider
        {
            {{endpointFunctionAttributeSourceCode}}
            public static Dependency<ProductEndpoint> {{endpointMethodName}}()
                =>
                default!;
        }

        namespace GarageGroup.Infra.Endpoint
        {
            using System;

            public interface IEndpoint
            {
            }

            {{endpointMetadataDeclarationSourceCode}}
        }

        namespace PrimeFuncPack
        {
            public sealed class Dependency<T>
            {
            }
        }
        """;
}
