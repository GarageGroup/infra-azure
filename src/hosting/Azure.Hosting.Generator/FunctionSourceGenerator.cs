using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator(LanguageNames.CSharp)]
internal sealed class FunctionSwaggerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(InnerBuildFunctionSourceCode);

        static void InnerBuildFunctionSourceCode(IncrementalGeneratorPostInitializationContext context)
            =>
            context.AddSource("RefreshableTokenCredentialFunction.g.cs", FunctionBuilder.BuildFunctionSourceCode());
    }
}