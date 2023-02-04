using System.Text;

namespace GGroupp.Infra;

partial class FunctionBuilder
{
    internal static string BuildFunctionSourceCode(this FunctionProviderMetadata provider, EndpointResolverMetadata resolver)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AddUsing(
            "System.Threading",
            "System.Threading.Tasks",
            "GGroupp.Infra.Endpoint",
            "Microsoft.Azure.Functions.Worker",
            "Microsoft.Azure.Functions.Worker.Http")
        .AppendCodeLine(
            $"partial class {provider.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"[Function({resolver.FunctionName.AsStringSourceCode(EmptyStringConstantSourceCode)})]",
            $"public static Task<HttpResponseData> {resolver.FunctionMethodName}(")
        .BeginArguments()
        .AppendCodeLine(
            $"{resolver.BuildHttpTriggerAttributeSourceCode()} HttpRequestData request,")
        .AppendCodeLine(
            "CancellationToken cancellationToken)")
        .EndArguments()
        .BeginLambda()
        .AppendCodeLine(
            $"{resolver.DependencyFieldName}.RunAzureFunctionAsync(request, cancellationToken);")
        .EndLambda()
        .EndCodeBlock()
        .Build();

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

            builder.Append(", ").Append(method.AsStringSourceCode());
        }

        if (string.IsNullOrEmpty(resolver.HttpRoute) is false)
        {
            builder = builder.Append(", Route = ").Append(resolver.HttpRoute.AsStringSourceCode());
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