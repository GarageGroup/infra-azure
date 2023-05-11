using GGroupp;

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
        .AppendCodeLine(
            $"public static class {swaggerUI.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            "[Function(\"GetSwaggerUI\")]",
            "public static HttpResponseData GetSwaggerUI(")
        .BeginArguments()
        .AppendCodeLine(
            "[HttpTrigger(AuthorizationLevel.Anonymous, \"GET\", Route = \"swagger\")] HttpRequestData request)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLine(
            "request.BuildStandardSwaggerUiResponse();")
        .EndLambda()
        .EndCodeBlock()
        .Build();
}