using System.Text;

namespace GarageGroup.Infra;

partial class FunctionBuilder
{
    internal static string BuildFunctionSourceCode(this FunctionProviderMetadata provider, EndpointResolverMetadata resolver)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AddUsing(
            "System.Threading",
            "System.Threading.Tasks",
            "GarageGroup.Infra.Endpoint",
            "Microsoft.Azure.Functions.Worker",
            "Microsoft.Azure.Functions.Worker.Http")
        .AppendCodeLine(
            $"partial class {provider.TypeName}")
        .BeginCodeBlock()
        .AppendObsoleteAttributeIfNecessary(
            resolver)
        .AppendCodeLine(
            $"[Function({resolver.FunctionName.AsStringSourceCodeOr()})]",
            $"public static Task<HttpResponseData> {resolver.FunctionMethodName}(")
        .BeginArguments()
        .AppendCodeLine(
            $"{resolver.BuildHttpTriggerAttributeSourceCode()} HttpRequestData request,")
        .AppendCodeLine(
            "CancellationToken cancellationToken)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLine(
            $"{provider.ProviderType.DisplayedTypeName}.{resolver.ResolverMethodName}().RunAzureFunctionAsync(request, cancellationToken);")
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendObsoleteAttributeIfNecessary(this SourceBuilder builder, EndpointResolverMetadata type)
    {
        if (type.ObsoleteData is null)
        {
            return builder;
        }

        var attributeBuilder = new StringBuilder("[Obsolete(").Append(type.ObsoleteData.Message.AsStringSourceCodeOr("null"));

        attributeBuilder = type.ObsoleteData.IsError switch
        {
            true => attributeBuilder.Append(", true"),
            false => attributeBuilder.Append(", false"),
            _ => attributeBuilder
        };

        if (string.IsNullOrEmpty(type.ObsoleteData.DiagnosticId) is false)
        {
            attributeBuilder = attributeBuilder.Append(", DiagnosticId = ").Append(type.ObsoleteData.DiagnosticId.AsStringSourceCodeOr());
        }

        if (string.IsNullOrEmpty(type.ObsoleteData.UrlFormat) is false)
        {
            attributeBuilder = attributeBuilder.Append(", UrlFormat = ").Append(type.ObsoleteData.UrlFormat.AsStringSourceCodeOr());
        }

        attributeBuilder = attributeBuilder.Append(")]");
        return builder.AddUsing("System").AppendCodeLine(attributeBuilder.ToString());
    }

    private static string BuildHttpTriggerAttributeSourceCode(this EndpointResolverMetadata resolver)
    {
        var authorizationLevelSourceCode = resolver.GetAuthorizationLevelSourceCode();
        var builder = new StringBuilder("[HttpTrigger(").Append(authorizationLevelSourceCode);

        foreach (var method in resolver.HttpMethodNames)
        {
            if (string.IsNullOrEmpty(method))
            {
                continue;
            }

            builder.Append(", ").Append(method.AsStringSourceCodeOrStringEmpty());
        }

        if (string.IsNullOrEmpty(resolver.HttpRoute) is false)
        {
            builder = builder.Append(", Route = ").Append(resolver.HttpRoute.AsStringSourceCodeOrStringEmpty());
        }

        return builder.Append(")]").ToString();
    }

    private static string GetAuthorizationLevelSourceCode(this EndpointResolverMetadata resolver)
        =>
        resolver.AuthorizationLevel switch
        {
            0 => "AuthorizationLevel.Anonymous",
            1 => "AuthorizationLevel.User",
            2 => "AuthorizationLevel.Function",
            3 => "AuthorizationLevel.System",
            4 => "AuthorizationLevel.Admin",
            _ => "(AuthorizationLevel)" + resolver.AuthorizationLevel
        };
}