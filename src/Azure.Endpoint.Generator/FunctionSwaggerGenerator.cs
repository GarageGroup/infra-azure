using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace GGroupp.Infra;

[Generator]
internal sealed class FunctionSwaggerGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var swaggerType = context.GetFunctionSwaggerType();
        if (swaggerType is null)
        {
            return;
        }

        var resolverTypes = context.GetFunctionProviderTypes().SelectMany(GetResolverTypes).ToArray();
        var swaggerSourceCode = swaggerType.BuildSwaggerSourceCode(resolverTypes);

        context.AddSource($"{swaggerType.TypeName}.g.cs", swaggerSourceCode);

        static IEnumerable<EndpointResolverMetadata> GetResolverTypes(FunctionProviderMetadata providerType)
            =>
            providerType.ResolverTypes;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}