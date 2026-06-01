using PrimeFuncPack;

namespace GarageGroup.Infra;

internal static class FunctionSwaggerUIBuilder
{
    internal static string BuildSwaggerUISourceCode(this FunctionSwaggerUIMetadata swaggerUI)
        =>
        new SourceBuilder(
            swaggerUI.Namespace)
        .AddUsing(
            "GarageGroup.Infra.Endpoint",
            "Microsoft.Azure.Functions.Worker",
            "Microsoft.Azure.Functions.Worker.Http")
        .AppendCodeLines(
            $"public static class {swaggerUI.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLines(
            "[Function(\"GetSwaggerUI\")]",
            "public static HttpResponseData GetSwaggerUI(")
        .BeginArguments()
        .AppendCodeLines(
            $"[HttpTrigger({swaggerUI.AuthorizationLevel.ToAuthorizationLevelSourceCode()}, \"GET\", Route = \"swagger\")] HttpRequestData request)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLines(
            "request.BuildStandardSwaggerUiResponse();")
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static string ToAuthorizationLevelSourceCode(this int authorizationLevel)
        =>
        authorizationLevel switch
        {
            0 => "AuthorizationLevel.Anonymous",
            1 => "AuthorizationLevel.User",
            2 => "AuthorizationLevel.Function",
            3 => "AuthorizationLevel.System",
            4 => "AuthorizationLevel.Admin",
            _ => "(AuthorizationLevel)" + authorizationLevel
        };
}
