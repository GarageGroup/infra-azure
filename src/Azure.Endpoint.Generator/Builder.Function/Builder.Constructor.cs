using System.Linq;
using GGroupp;

namespace GarageGroup.Infra;

partial class FunctionBuilder
{
    internal static string BuildConstructorSourceCode(this FunctionProviderMetadata provider)
        =>
        provider.ResolverTypes.Any() ? provider.BuildNotEmptyConstructorSourceCode() : provider.BuildEmptyConstructorSourceCode();

    private static string BuildEmptyConstructorSourceCode(this FunctionProviderMetadata provider)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AppendCodeLine(
            $"public static class {provider.TypeName}")
        .BeginCodeBlock()
        .EndCodeBlock()
        .Build();

    private static string BuildNotEmptyConstructorSourceCode(this FunctionProviderMetadata provider)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AddUsings(
            provider.ProviderType.AllNamespaces)
        .AddUsing(
            "PrimeFuncPack")
        .AppendCodeLine(
            $"public static partial class {provider.TypeName}")
        .BeginCodeBlock()
        .AppendStaticConstructor(provider)
        .AppendDependencyFields(provider)
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendStaticConstructor(this SourceBuilder builder, FunctionProviderMetadata provider)
    {
        builder = builder.AppendCodeLine($"static {provider.TypeName}()");

        if (provider.ResolverTypes.Count is 1)
        {
            var line = GetResolverInitializationLine(provider.ResolverTypes[0]);
            return builder.BeginLambda().AppendCodeLine(line).EndLambda();
        }

        builder = builder.BeginCodeBlock();

        foreach (var initializationLine in provider.ResolverTypes.Select(GetResolverInitializationLine))
        {
            builder = builder.AppendCodeLine(initializationLine);
        }

        return builder.EndCodeBlock();

        string GetResolverInitializationLine(EndpointResolverMetadata resolver)
            =>
            $"{resolver.DependencyFieldName} = {provider.ProviderType.DisplayedTypeName}.{resolver.ResolverMethodName}();";
    }

    private static SourceBuilder AppendDependencyFields(this SourceBuilder builder, FunctionProviderMetadata provider)
    {
        foreach (var resolver in provider.ResolverTypes)
        {
            builder = builder.AddUsings(resolver.EndpointType.AllNamespaces).AppendEmptyLine().AppendCodeLine(
                $"private static readonly Dependency<{resolver.EndpointType.DisplayedTypeName}> {resolver.DependencyFieldName};");
        }

        return builder;
    }
}