using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator(LanguageNames.CSharp)]
internal sealed class FunctionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.CompilationProvider.SelectMany(SourceGeneratorExtensions.GetFunctionProviderTypes);
        context.RegisterSourceOutput(types, AddSources);
    }

    private static void AddSources(SourceProductionContext context, FunctionProviderMetadata providerType)
    {
        var constructorSourceCode = providerType.BuildConstructorSourceCode();
        context.AddSource($"{providerType.TypeName}.g.cs", constructorSourceCode);

        foreach (var resolverType in providerType.ResolverTypes)
        {
            var functionSourceCode = providerType.BuildFunctionSourceCode(resolverType);
            context.AddSource($"{providerType.TypeName}.{resolverType.FunctionMethodName}.g.cs", functionSourceCode);
        }
    }
}