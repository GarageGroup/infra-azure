namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

internal static partial class FunctionSourceGeneratorSource
{
    internal const string ExecuteSwaggerUI_AssemblyAttribute_GeneratesSwaggerUIFunction_SourceCode
        =
        """
        using GarageGroup.Infra;

        [assembly: EndpointFunctionSwaggerUI]

        namespace Demo.Functions;

        public static class DummyType
        {
        }
        """;

    internal const string ExecuteSwaggerUI_AssemblyAttribute_GeneratesSwaggerUIFunction_ExpectedSource
        =
        """
        // Auto-generated code by PrimeFuncPack
        #nullable enable

        using GarageGroup.Infra.Endpoint;
        using Microsoft.Azure.Functions.Worker;
        using Microsoft.Azure.Functions.Worker.Http;

        namespace Demo.Functions;

        public static class SwaggerProviderSwaggerUI
        {
            [Function("GetSwaggerUI")]
            public static HttpResponseData GetSwaggerUI(
                [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "swagger")] HttpRequestData request)
                =>
                request.BuildStandardSwaggerUiResponse();
        }
        """;

    internal const string ExecuteSwaggerUI_LevelSpecified_UsesSpecifiedAuthorizationLevel_SourceCode
        =
        """
        using GarageGroup.Infra;

        [assembly: EndpointFunctionSwaggerUI(FunctionAuthorizationLevel.System)]

        namespace Demo.Functions;

        public static class DummyType
        {
        }
        """;

    internal const string ExecuteSwaggerUI_LevelSpecified_UsesSpecifiedAuthorizationLevel_ExpectedSource
        =
        """
        // Auto-generated code by PrimeFuncPack
        #nullable enable

        using GarageGroup.Infra.Endpoint;
        using Microsoft.Azure.Functions.Worker;
        using Microsoft.Azure.Functions.Worker.Http;

        namespace Demo.Functions;

        public static class SwaggerProviderSwaggerUI
        {
            [Function("GetSwaggerUI")]
            public static HttpResponseData GetSwaggerUI(
                [HttpTrigger(AuthorizationLevel.System, "GET", Route = "swagger")] HttpRequestData request)
                =>
                request.BuildStandardSwaggerUiResponse();
        }
        """;
}
