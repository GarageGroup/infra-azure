using System.Linq;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator(LanguageNames.CSharp)]
internal sealed class FunctionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var functionData = context.CompilationProvider.Select(SourceGeneratorExtensions.GetFunctionData);
        context.RegisterSourceOutput(functionData, AddSources);
    }

    private static void AddSources(SourceProductionContext context, FunctionData? functionData)
    {
        if (functionData is null)
        {
            return;
        }

        var sourceCode = FunctionBuilder.BuildFunctionSourceCode(functionData);
        context.AddSource("RefreshableTokenCredentialFunction.g.cs", sourceCode);
    }
}