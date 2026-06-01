using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static FunctionSwaggerMetadata? GetFunctionSwaggerType(
        this Compilation compilation, CancellationToken cancellationToken)
    {
        var swaggerAttribute = compilation.Assembly.GetAttributes().FirstOrDefault(IsFunctionSwaggerAttribute);
        if (swaggerAttribute is null)
        {
            return null;
        }

        var @namespace = GetSwaggerDefaultNamespace(compilation, cancellationToken);
        var authorizationLevel = GetAuthorizationLevelOrDefault(swaggerAttribute);

        return new(@namespace, "SwaggerProviderSwagger", authorizationLevel);

        static bool IsFunctionSwaggerAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "EndpointFunctionSwaggerAttribute") is true;
    }

    private static int GetAuthorizationLevelOrDefault(this AttributeData swaggerAttribute)
    {
        var constructorLevelValue = swaggerAttribute.ConstructorArguments.FirstOrDefault().Value;
        if (TryGetAuthorizationLevel(constructorLevelValue, out var constructorLevel))
        {
            return constructorLevel;
        }

        var namedLevelValue = swaggerAttribute.NamedArguments
            .FirstOrDefault(static pair => pair.Key == "Level")
            .Value
            .Value;

        return TryGetAuthorizationLevel(namedLevelValue, out var namedLevel)
            ? namedLevel
            : DefaultFunctionAuthorizationLevel;
    }

    private static bool TryGetAuthorizationLevel(object? levelValue, out int authorizationLevel)
    {
        var numericLevel = levelValue switch
        {
            int intLevel => intLevel,
            byte byteLevel => byteLevel,
            short shortLevel => shortLevel,
            long longLevel => (int)longLevel,
            uint uintLevel => (int)uintLevel,
            ulong ulongLevel => (int)ulongLevel,
            _ => (int?)null
        };

        if (numericLevel is not null)
        {
            authorizationLevel = numericLevel.Value;
            return true;
        }

        var levelSource = levelValue?.ToString();
        if (int.TryParse(levelSource, out var parsedLevel))
        {
            authorizationLevel = parsedLevel;
            return true;
        }

        var enumLevel = levelSource switch
        {
            "Anonymous" => 0,
            "User" => 1,
            "Function" => 2,
            "System" => 3,
            "Admin" => 4,
            _ when levelSource?.EndsWith(".Anonymous") is true => 0,
            _ when levelSource?.EndsWith(".User") is true => 1,
            _ when levelSource?.EndsWith(".Function") is true => 2,
            _ when levelSource?.EndsWith(".System") is true => 3,
            _ when levelSource?.EndsWith(".Admin") is true => 4,
            _ => (int?)null
        };

        if (enumLevel is null)
        {
            authorizationLevel = default;
            return false;
        }

        authorizationLevel = enumLevel.Value;
        return true;
    }

    private static string GetSwaggerDefaultNamespace(Compilation compilation, CancellationToken cancellationToken)
    {
        var visitor = new ExportedTypesCollector(cancellationToken);
        visitor.VisitNamespace(compilation.GlobalNamespace);

        return visitor.GetExportedTypes()
            .Select(static typeSymbol => typeSymbol.ContainingNamespace.ToString())
            .FirstOrDefault(static @namespace => string.IsNullOrWhiteSpace(@namespace) is false)
            ?? DefaultNamespace;
    }
}
