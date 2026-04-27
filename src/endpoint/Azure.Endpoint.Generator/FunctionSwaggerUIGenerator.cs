using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator(LanguageNames.CSharp)]
internal sealed class FunctionSwaggerUIGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.CompilationProvider.Select(SourceGeneratorExtensions.GetFunctionSwaggerUIType);
        context.RegisterSourceOutput(types, AddSources);
    }

    private static void AddSources(SourceProductionContext context, FunctionSwaggerUIMetadata? swaggerUIType)
    {
        if (swaggerUIType is null)
        {
            return;
        }

        var swaggerUISourceCode = swaggerUIType.BuildSwaggerUISourceCode();
        context.AddSource($"{swaggerUIType.TypeName}.g.cs", swaggerUISourceCode);
    }
}