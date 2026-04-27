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
            "[HttpTrigger(AuthorizationLevel.Anonymous, \"GET\", Route = \"swagger\")] HttpRequestData request)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLines(
            "request.BuildStandardSwaggerUiResponse();")
        .EndLambda()
        .EndCodeBlock()
        .Build();
}