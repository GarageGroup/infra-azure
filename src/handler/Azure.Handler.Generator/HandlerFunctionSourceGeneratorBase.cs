using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

public abstract class HandlerFunctionSourceGeneratorBase : IIncrementalGenerator
{
    protected abstract HandlerFunctionProvider GetFunctionProvider();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var handlerFunctionProvider = GetFunctionProvider();
        context.RegisterSourceOutput(context.CompilationProvider, InnerGenerateSource);

        void InnerGenerateSource(SourceProductionContext context, Compilation compilation)
        {
            foreach (var providerType in handlerFunctionProvider.GetFunctionProviderTypes(compilation, context.CancellationToken))
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
    }
}