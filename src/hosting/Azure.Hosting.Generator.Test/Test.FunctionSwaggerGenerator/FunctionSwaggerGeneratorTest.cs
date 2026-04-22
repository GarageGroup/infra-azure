using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace GarageGroup.Infra.Azure.Hosting.Generator.Test;

public static partial class FunctionSwaggerGeneratorTest
{
    public static TheoryData<string> SourceCodes
        =>
        [
            string.Empty,
            """
            namespace Some.Test;

            public static class Stub
            {
            }
            """
        ];

    private static readonly IReadOnlyList<MetadataReference> MetadataReferences
        =
        [
            ..((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).OrEmpty().Split(Path.PathSeparator).Select(CreateFromFile)
        ];

    private static MetadataReference CreateFromFile(string path)
        =>
        MetadataReference.CreateFromFile(path);

    private static GeneratorDriverRunResult RunGenerator(string sourceCode)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "Azure.Hosting.Generator.DynamicTests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(sourceCode, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest))
            ],
            references: MetadataReferences,
            options: new(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver generatorDriver = CSharpGeneratorDriver.Create(CreateGenerator());
        generatorDriver = generatorDriver.RunGenerators(compilation);

        return generatorDriver.GetRunResult();
    }

    private static ISourceGenerator CreateGenerator()
    {
        var assembly = Assembly.Load("GarageGroup.Infra.Azure.Hosting.Generator");
        var generatorType = assembly.GetType("GarageGroup.Infra.FunctionSwaggerGenerator", throwOnError: true)!;
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
