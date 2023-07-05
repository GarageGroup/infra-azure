using System;
using System.Text;

namespace GarageGroup.Infra;

partial class HandlerFunctionBuilder
{
    internal static string BuildFunctionSourceCode(this FunctionProviderMetadata provider, HandlerResolverMetadata resolver)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AddUsing(
            "System.Threading",
            "System.Threading.Tasks",
            "GarageGroup.Infra",
            "Microsoft.Azure.Functions.Worker")
        .AppendCodeLine(
            $"partial class {provider.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"[Function({resolver.FunctionName.AsStringSourceCode()})]",
            $"public static {resolver.GetResponseType()} {resolver.FunctionMethodName}(")
        .BeginArguments()
        .AppendAzureFunctionBodyArguments(
            resolver)
        .EndArguments()
        .BeginLambda()
        .AppendAzureFunctionBody(
            provider, resolver)
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static string GetResponseType(this HandlerResolverMetadata resolver)
        =>
        resolver.FunctionData switch
        {
            HttpFunctionData    => "Task<HttpResponseData>",
            _                   => "Task"
        };

    private static SourceBuilder AppendAzureFunctionBodyArguments(this SourceBuilder builder, HandlerResolverMetadata resolver)
    {
        if (resolver.FunctionData is HttpFunctionData httpFunctionData)
        {
            return builder.AddUsing("Microsoft.Azure.Functions.Worker.Http").AppendCodeLine(
                $"{httpFunctionData.BuildHttpTriggerAttributeSourceCode()} HttpRequestData requestData,",
                "CancellationToken cancellationToken)");
        }

        builder = builder.AddUsing("System.Text.Json");

        var triggerAttributeSourceCode = resolver.FunctionData switch
        {
            EventGridFunctionData => "[EventGridTrigger]",
            ServiceBusFunctionData busFunctionData => busFunctionData.BuildServiceBusTriggerAttributeSourceCode(),
            _ => throw new InvalidOperationException($"An unexpected function data type: {resolver.FunctionData.GetType()}")
        };

        return builder.AppendCodeLine(
            $"{triggerAttributeSourceCode} JsonElement requestData,",
            "FunctionContext context,",
            "CancellationToken cancellationToken)");
    }

    private static SourceBuilder AppendAzureFunctionBody(this SourceBuilder builder, FunctionProviderMetadata provider, HandlerResolverMetadata resolver)
    {
        return builder.AddUsings(
                resolver.HandlerType.AllNamespaces)
            .AddUsings(
                resolver.InputType.AllNamespaces)
            .AddUsings(
                resolver.OutputType.AllNamespaces)
            .AppendCodeLine(
                BuildFirstLine(provider, resolver))
            .BeginArguments()
            .AppendCodeLine(
                BuildSecondLine(resolver))
            .EndArguments();

        static string BuildFirstLine(FunctionProviderMetadata provider, HandlerResolverMetadata resolver)
            =>
            new StringBuilder(
                provider.ProviderType.DisplayedTypeName)
            .Append(
                '.')
            .Append(
                resolver.ResolverMethodName)
            .Append(
                resolver.FunctionData is HttpFunctionData ? "().RunHttpFunctionAsync<" : "().RunAzureFunctionAsync<")
            .Append(
                resolver.HandlerType.DisplayedTypeName)
            .Append(
                ", ")
            .Append(
                resolver.InputType.DisplayedTypeName)
            .Append(
                ", ")
            .Append(
                resolver.OutputType.DisplayedTypeName)
            .Append(
                ">(")
            .ToString();

        static string BuildSecondLine(HandlerResolverMetadata resolver)
        {
            var lineBuilder = new StringBuilder("requestData");

            if (resolver.FunctionData is HttpFunctionData)
            {
                return lineBuilder.Append(", cancellationToken);").ToString();
            }

            if (string.IsNullOrEmpty(resolver.JsonRootPath) is false)
            {
                lineBuilder = lineBuilder.Append(".GetProperty(").Append(resolver.JsonRootPath.AsStringSourceCode(EmptyStringSourceCode)).Append(')');
            }

            return lineBuilder.Append(", context, cancellationToken);").ToString();
        }
    }

    private static string BuildServiceBusTriggerAttributeSourceCode(this ServiceBusFunctionData functionData)
    {
        var queueNameSourceCode = functionData.QueueName.AsStringSourceCode();
        var builder = new StringBuilder("[ServiceBusTrigger(").Append(queueNameSourceCode);

        if (string.IsNullOrEmpty(functionData.Connection) is false)
        {
            builder = builder.Append(", Connection = ").Append(functionData.Connection.AsStringSourceCode());
        }

        return builder.Append(")]").ToString();
    }

    private static string BuildHttpTriggerAttributeSourceCode(this HttpFunctionData functionData)
    {
        var authorizationLevelSourceCode = functionData.GetAuthorizationLevelSourceCode();
        var builder = new StringBuilder("[HttpTrigger(")
            .Append(authorizationLevelSourceCode)
            .Append(", ")
            .Append(functionData.Method.AsStringSourceCode());

        if (string.IsNullOrEmpty(functionData.FunctionRoute) is false)
        {
            builder = builder.Append(", Route = ").Append(functionData.FunctionRoute.AsStringSourceCode());
        }

        return builder.Append(")]").ToString();
    }

    private static string GetAuthorizationLevelSourceCode(this HttpFunctionData functionData)
        =>
        functionData.AuthorizationLevel switch
        {
            0 => "AuthorizationLevel.Anonymous",
            1 => "AuthorizationLevel.User",
            2 => "AuthorizationLevel.Function",
            3 => "AuthorizationLevel.System",
            4 => "AuthorizationLevel.Admin",
            _ => "(AuthorizationLevel)" + functionData.AuthorizationLevel
        };
}