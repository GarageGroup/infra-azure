using System.Text;
using GGroupp;

namespace GarageGroup.Infra;

partial class HandlerFunctionBuilder
{
    internal static string BuildFunctionSourceCode(this FunctionProviderMetadata provider, HandlerResolverMetadata resolver)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AddUsing(
            "System.Text.Json",
            "System.Threading.Tasks",
            "GarageGroup.Infra",
            "Microsoft.Azure.Functions.Worker")
        .AppendCodeLine(
            $"partial class {provider.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"[Function({resolver.FunctionName.AsStringSourceCode(EmptyStringConstantSourceCode)})]",
            $"public static Task {resolver.FunctionMethodName}(")
        .BeginArguments()
        .AddUsing(
            resolver.FunctionAttributeNamespace)
        .AppendCodeLine(
            $"[{resolver.FunctionAttributeTypeName}] JsonElement eventData, FunctionContext context)")
        .EndArguments()
        .BeginLambda()
        .AppendAzureFunctionBody(
            resolver)
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendAzureFunctionBody(this SourceBuilder builder, HandlerResolverMetadata resolver)
    {
        return builder.AddUsings(
                resolver.HandlerType.AllNamespaces)
            .AddUsings(
                resolver.EvendDataType.AllNamespaces)
            .AppendCodeLine(
                BuildFirstLine(resolver))
            .BeginArguments()
            .AppendCodeLine(
                BuildSecondLine(resolver))
            .EndArguments();

        static string BuildFirstLine(HandlerResolverMetadata resolver)
            =>
            new StringBuilder(
                resolver.DependencyFieldName)
            .Append(
                ".RunAzureFunctionAsync<")
            .Append(
                resolver.HandlerType.DisplayedTypeName)
            .Append(
                ", ")
            .Append(
                resolver.EvendDataType.DisplayedTypeName)
            .Append(
                ">(")
            .ToString();

        static string BuildSecondLine(HandlerResolverMetadata resolver)
        {
            var lineBuilder = new StringBuilder("eventData");
            if (string.IsNullOrEmpty(resolver.JsonRootPath) is false)
            {
                lineBuilder = lineBuilder.Append(".GetProperty(").Append(resolver.JsonRootPath.AsStringSourceCode()).Append(')');
            }

            return lineBuilder.Append(", context);").ToString();
        }
    }
}