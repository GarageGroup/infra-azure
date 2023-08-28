using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator]
internal sealed class FunctionSwaggerUIGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var swaggerUIType = context.GetFunctionSwaggerUIType();
        if (swaggerUIType is null)
        {
            return;
        }

        var swaggerUISourceCode = swaggerUIType.BuildSwaggerUISourceCode();
        context.AddSource($"{swaggerUIType.TypeName}.g.cs", swaggerUISourceCode);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}