using System.Collections.Generic;

namespace GGroupp.Infra;

internal static class FunctionSwaggerBuilder
{
    internal static string BuildSwaggerSourceCode(
        this FunctionSwaggerMetadata swagger, IReadOnlyCollection<EndpointResolverMetadata> resolverTypes)
        =>
        new SourceBuilder(
            swagger.Namespace)
        .AddUsing(
            "GGroupp.Infra.Endpoint",
            "Microsoft.Azure.Functions.Worker",
            "Microsoft.Azure.Functions.Worker.Http")
        .AppendCodeLine(
            $"public static class {swagger.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            "[Function(\"GetSwaggerDocument\")]",
            "public static HttpResponseData GetSwaggerDocument(")
        .BeginArguments()
        .AppendCodeLine(
            "[HttpTrigger(AuthorizationLevel.Anonymous, \"GET\", Route = \"swagger/swagger.{format}\")] HttpRequestData request, string? format)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLine(
            "request.FunctionContext")
        .BeginArguments()
        .AppendCodeLine(
            ".GetSwaggerOption(\"Swagger\")",
            ".CreateBuilder(format)")
        .AppendEndpoints(
            resolverTypes)
        .AppendCodeLine(
            ".BuildResponse(request);")
        .EndArguments()
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendEndpoints(this SourceBuilder builder, IReadOnlyCollection<EndpointResolverMetadata> resolverTypes)
    {
        foreach (var resolver in resolverTypes)
        {
            builder = builder.AddUsings(resolver.EndpointType.AllNamespaces).AppendCodeLine(
                $".AddFunctionEndpoint({resolver.EndpointType.DisplayedTypeName}.GetEndpointMetadata())");
        }

        return builder;
    }
}