using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GarageGroup.Infra.Azure.Endpoint.Generator.Test;

public static partial class FunctionSourceGeneratorTest
{
    private static readonly IReadOnlyList<MetadataReference> MetadataReferences
        =
        [
            ..((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).OrEmpty().Split(Path.PathSeparator).Select(CreateFromFile),
            CreateFromType<EndpointFunctionAttribute>()
        ];

    private static MetadataReference CreateFromFile(string path)
        =>
        MetadataReference.CreateFromFile(path);

    private static MetadataReference CreateFromType<T>()
        =>
        MetadataReference.CreateFromFile(typeof(T).Assembly.Location);

    private static GeneratorDriverRunResult RunGenerator(string sourceCode)
        =>
        RunGenerator(sourceCode, "GarageGroup.Infra.FunctionSourceGenerator");

    private static GeneratorDriverRunResult RunSwaggerGenerator(string sourceCode)
        =>
        RunGenerator(sourceCode, "GarageGroup.Infra.FunctionSwaggerGenerator");

    private static GeneratorDriverRunResult RunSwaggerUIGenerator(string sourceCode)
        =>
        RunGenerator(sourceCode, "GarageGroup.Infra.FunctionSwaggerUIGenerator");

    private static GeneratorDriverRunResult RunGenerator(string sourceCode, string generatorTypeName)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "Azure.Endpoint.Generator.DynamicTests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(sourceCode, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest))
            ],
            references: MetadataReferences,
            options: new(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver generatorDriver = CSharpGeneratorDriver.Create(CreateGenerator(generatorTypeName));
        generatorDriver = generatorDriver.RunGenerators(compilation);

        return generatorDriver.GetRunResult();
    }

    private static ISourceGenerator CreateGenerator(string generatorTypeName)
    {
        var assembly = Assembly.Load("GarageGroup.Infra.Azure.Endpoint.Generator");
        var generatorType = assembly.GetType(generatorTypeName, throwOnError: true)!;
        var generator = Activator.CreateInstance(generatorType, nonPublic: true)!;

        return generator switch
        {
            ISourceGenerator sourceGenerator => sourceGenerator,
            IIncrementalGenerator incrementalGenerator => incrementalGenerator.AsSourceGenerator(),
            _ => throw new InvalidOperationException($"Unsupported generator type: {generatorType.FullName}")
        };
    }

    private static string NormalizeNewLines(string source)
        =>
        source.Replace("\r\n", "\n").Trim();

    private static string OrEmpty(this string? value)
        =>
        value ?? string.Empty;
}