using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static FunctionSwaggerUIMetadata? GetFunctionSwaggerUIType(
        this Compilation compilation, CancellationToken cancellationToken)
    {
        var swaggerUiAttribute = compilation.Assembly.GetAttributes().FirstOrDefault(IsFunctionSwaggerUIAttribute);
        if (swaggerUiAttribute is null)
        {
            return null;
        }

        var @namespace = GetSwaggerUIDefaultNamespace(compilation, cancellationToken);
        var authorizationLevel = swaggerUiAttribute.GetAuthorizationLevelOrDefault();

        return new(@namespace, "SwaggerProviderSwaggerUI", authorizationLevel);

        static bool IsFunctionSwaggerUIAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionSwaggerUIAttribute") is true;
    }

    private static string GetSwaggerUIDefaultNamespace(Compilation compilation, CancellationToken cancellationToken)
    {
        var visitor = new ExportedTypesCollector(cancellationToken);
        visitor.VisitNamespace(compilation.GlobalNamespace);

        return visitor.GetExportedTypes()
            .Select(static typeSymbol => typeSymbol.ContainingNamespace.ToString())
            .FirstOrDefault(static @namespace => string.IsNullOrWhiteSpace(@namespace) is false)
            ?? DefaultNamespace;
    }
}
