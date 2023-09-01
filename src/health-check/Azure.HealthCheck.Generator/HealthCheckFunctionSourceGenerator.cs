using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator]
internal sealed class HealthCheckFunctionSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var providerType in context.GetHealthCheckTypes())
        {
            var sourceCode = providerType.BuildFunctionSourceCode();
            context.AddSource($"{providerType.TypeName}.g.cs", sourceCode);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}