using System.Collections.Generic;
using System.Linq;
using PrimeFuncPack;

namespace GarageGroup.Infra;

internal static class FunctionSwaggerBuilder
{
    internal static string BuildSwaggerSourceCode(
        this FunctionSwaggerMetadata swagger, IReadOnlyCollection<EndpointResolverMetadata>? resolverTypes)
        =>
        new SourceBuilder(
            swagger.Namespace)
        .AddUsing(
            "System.Threading",
            "System.Threading.Tasks",
            "GarageGroup.Infra.Endpoint",
            "Microsoft.Azure.Functions.Worker",
            "Microsoft.Azure.Functions.Worker.Http")
        .AppendCodeLines(
            $"public static class {swagger.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLines(
            "[Function(\"GetSwaggerDocument\")]",
            "public static Task<HttpResponseData> GetSwaggerDocumentAsync(")
        .BeginArguments()
        .AppendCodeLines(
            "[HttpTrigger(AuthorizationLevel.Anonymous, \"GET\", Route = \"swagger/swagger.{format}\")] HttpRequestData request,")
        .AppendCodeLines("string? format,")
        .AppendCodeLines("CancellationToken cancellationToken)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLines(
            "request.CreateStandardSwaggerBuilder()")
        .AppendEndpoints(
            resolverTypes)
        .AppendCodeLines(
            ".BuildResponseAsync(request, format, cancellationToken);")
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendEndpoints(this SourceBuilder builder, IReadOnlyCollection<EndpointResolverMetadata>? resolverTypes)
    {
        if (resolverTypes?.Count is not > 0)
        {
            return builder;
        }

        foreach (var resolver in resolverTypes)
        {
            if (resolver.IsSwaggerHidden)
            {
                continue;
            }

            builder = builder.AddUsing(resolver.EndpointType.AllNamespaces.ToArray()).AppendCodeLines(
                $".AddFunctionEndpoint({resolver.EndpointType.DisplayedTypeName}.GetEndpointMetadata())");
        }

        return builder;
    }
}