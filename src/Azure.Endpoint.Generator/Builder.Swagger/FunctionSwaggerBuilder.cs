using System.Collections.Generic;
using GGroupp;

namespace GarageGroup.Infra;

internal static class FunctionSwaggerBuilder
{
    internal static string BuildSwaggerSourceCode(
        this FunctionSwaggerMetadata swagger, IReadOnlyCollection<EndpointResolverMetadata> resolverTypes)
        =>
        new SourceBuilder(
            swagger.Namespace)
        .AddUsing(
            "System.Threading",
            "System.Threading.Tasks",
            "GarageGroup.Infra.Endpoint",
            "Microsoft.Azure.Functions.Worker",
            "Microsoft.Azure.Functions.Worker.Http")
        .AppendCodeLine(
            $"public static class {swagger.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            "[Function(\"GetSwaggerDocument\")]",
            "public static Task<HttpResponseData> GetSwaggerDocumentAsync(")
        .BeginArguments()
        .AppendCodeLine(
            "[HttpTrigger(AuthorizationLevel.Anonymous, \"GET\", Route = \"swagger/swagger.{format}\")] HttpRequestData request,")
        .AppendCodeLine("string? format,")
        .AppendCodeLine("CancellationToken cancellationToken)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLine(
            "request.CreateStandardSwaggerBuilder()")
        .AppendEndpoints(
            resolverTypes)
        .AppendCodeLine(
            ".BuildResponseAsync(request, format, cancellationToken);")
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