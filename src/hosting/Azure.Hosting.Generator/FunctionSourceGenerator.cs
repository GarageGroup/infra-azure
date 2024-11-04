using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator]
internal sealed class FunctionSwaggerGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
        =>
        context.AddSource("RefreshableTokenCredentialFunction.g.cs", FunctionBuilder.BuildFunctionSourceCode());

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}