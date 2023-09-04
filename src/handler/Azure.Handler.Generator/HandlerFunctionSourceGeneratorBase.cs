using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

public abstract class HandlerFunctionSourceGeneratorBase : ISourceGenerator
{
    protected abstract HandlerFunctionProvider GetFunctionProvider();

    public void Execute(GeneratorExecutionContext context)
    {
        var handlerFunctionProvider = GetFunctionProvider();
        foreach (var providerType in handlerFunctionProvider.GetFunctionProviderTypes(context))
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