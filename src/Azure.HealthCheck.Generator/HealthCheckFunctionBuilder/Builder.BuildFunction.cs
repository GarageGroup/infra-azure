using System.Text;
using GGroupp;

namespace GarageGroup.Infra;

partial class HealthCheckFunctionBuilder
{
    internal static string BuildFunctionSourceCode(this HealthCheckMetadata metadata)
        =>
        new SourceBuilder(
            metadata.Namespace)
        .AddUsing(
            "Microsoft.Azure.Functions.Worker",
            "Microsoft.Azure.Functions.Worker.Http")
        .AppendCodeLine(
            $"public static class {metadata.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"[Function({metadata.FunctionName.AsStringSourceCode()})]",
            $"public static void Run({metadata.BuildHttpTriggerAttributeSourceCode()} HttpRequestData request)")
        .BeginLambda()
        .AppendCodeLine(
            $"HealthCheckFunction.Run(request);")
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static string BuildHttpTriggerAttributeSourceCode(this HealthCheckMetadata resolver)
    {
        var authorizationLevelSourceCode = resolver.GetAuthorizationLevelSourceCode();
        var builder = new StringBuilder("[HttpTrigger(").Append(authorizationLevelSourceCode).Append(", \"GET\"");

        if (string.IsNullOrEmpty(resolver.FunctionRoute) is false)
        {
            builder = builder.Append(", Route = ").Append(resolver.FunctionRoute.AsStringSourceCode());
        }

        return builder.Append(")]").ToString();
    }

    private static string GetAuthorizationLevelSourceCode(this HealthCheckMetadata resolver)
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