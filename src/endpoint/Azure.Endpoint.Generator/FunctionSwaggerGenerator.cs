using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GarageGroup.Infra;

[Generator(LanguageNames.CSharp)]
internal sealed class FunctionSwaggerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.CompilationProvider.Select(InnerGetSwaggerType);
        context.RegisterSourceOutput(types, AddSources);

        static SwaggerMetadata InnerGetSwaggerType(Compilation compilation, CancellationToken cancellationToken)
        {
            var swaggerType = compilation.GetFunctionSwaggerType(cancellationToken);
            if (swaggerType is null)
            {
                return new();
            }

            return new()
            {
                SwaggerType = swaggerType,
                ResolverTypes = compilation.GetFunctionProviderTypes(cancellationToken).SelectMany(GetResolverTypes).ToArray()
            };
        }

        static IEnumerable<EndpointResolverMetadata> GetResolverTypes(FunctionProviderMetadata providerType)
            =>
            providerType.ResolverTypes;
    }

    private static void AddSources(SourceProductionContext context, SwaggerMetadata metadata)
    {
        if (metadata.SwaggerType is null)
        {
            return;
        }

        var swaggerSourceCode = metadata.SwaggerType.BuildSwaggerSourceCode(metadata.ResolverTypes);
        context.AddSource($"{metadata.SwaggerType.TypeName}.g.cs", swaggerSourceCode);
    }

    private sealed class SwaggerMetadata
    {
        public FunctionSwaggerMetadata? SwaggerType { get; set; }

        public IReadOnlyCollection<EndpointResolverMetadata>? ResolverTypes { get; set; }
    };
}