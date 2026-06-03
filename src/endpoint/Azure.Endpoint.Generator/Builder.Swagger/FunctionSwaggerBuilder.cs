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
            $"{swagger.BuildHttpTriggerAttributeSourceCode()} HttpRequestData request,")
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

    private static string BuildHttpTriggerAttributeSourceCode(this FunctionSwaggerMetadata swagger)
        =>
        $"[HttpTrigger({swagger.AuthorizationLevel.ToAuthorizationLevelSourceCode()}, \"GET\", Route = \"swagger/swagger.{{format}}\")]";

    private static SourceBuilder AppendEndpoints(this SourceBuilder builder, IReadOnlyCollection<EndpointResolverMetadata>? resolverTypes)
    {
        var swaggerTypes = resolverTypes?.Where(IsNotSwaggerHidden).ToArray();
        if (swaggerTypes?.Length is not > 0)
        {
            return builder;
        }

        var groups = swaggerTypes.GroupBy(
            static resolver => new
            {
                EndpointTypeNamespace = string.Join(".", resolver.EndpointType.AllNamespaces),
                EndpointTypeDisplayedName = resolver.EndpointType.DisplayedTypeName,
                resolver.IsEndpointSetOperation
            });

        foreach (var group in groups)
        {
            builder = builder.AddUsing(group.First().EndpointType.AllNamespaces.ToArray());
            var isAuthorizationRequired = group.Any(static resolver => resolver.IsAuthorizationRequired);

            if (group.Key.IsEndpointSetOperation is false)
            {
                builder = builder.AppendCodeLines(
                    $".AddFunctionEndpoint({group.Key.EndpointTypeDisplayedName}.GetEndpointMetadata(), isAuthorizationRequired: " +
                    $"{isAuthorizationRequired.ToBooleanSourceCode()})");

                continue;
            }

            builder = builder.AppendCodeLines(
                $".AddFunctionEndpoints({group.Key.EndpointTypeDisplayedName}.Metadata, isAuthorizationRequired: " +
                $"{isAuthorizationRequired.ToBooleanSourceCode()})");
        }

        return builder;

        static bool IsNotSwaggerHidden(EndpointResolverMetadata resolver)
            =>
            resolver.IsSwaggerHidden is false;
    }

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

    private static string ToBooleanSourceCode(this bool value)
        =>
        value ? "true" : "false";
}