using Microsoft.CodeAnalysis;

namespace GGroupp.Infra;

[Generator]
internal sealed class FunctionSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var providerType in context.GetFunctionProviderTypes())
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

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}