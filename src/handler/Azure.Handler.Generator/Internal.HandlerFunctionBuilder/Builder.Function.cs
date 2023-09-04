using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GarageGroup.Infra;

partial class HandlerFunctionBuilder
{
    internal static string BuildFunctionSourceCode(this FunctionProviderMetadata provider, HandlerResolverMetadata resolver)
        =>
        new SourceBuilder(
            provider.Namespace)
        .AddUsing(
            "GarageGroup.Infra",
            "Microsoft.Azure.Functions.Worker")
        .AddUsings(
            resolver.FunctionSpecificData.Namespaces)
        .AppendCodeLine(
            $"partial class {provider.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"[Function({resolver.FunctionName.AsStringSourceCodeOr()})]",
            $"public static {resolver.FunctionSpecificData.ResponseTypeDisplayName} {resolver.FunctionMethodName}(")
        .BeginArguments()
        .AppendAzureFunctionBodyArguments(
            resolver)
        .EndArguments()
        .BeginLambda()
        .AppendAzureFunctionBody(
            provider, resolver)
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendAzureFunctionBodyArguments(this SourceBuilder builder, HandlerResolverMetadata resolver)
    {
        if (resolver.FunctionSpecificData.Arguments.Any() is false)
        {
            return builder.AppendCodeLine(")");
        }

        var arguments = resolver.FunctionSpecificData.Arguments.OrderBy(InnerGetOrderNumber).ToArray();

        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = arguments[i];
            var lineBuilder = new StringBuilder();

            builder.AddUsings(argument.Namespaces);
            var attributesSourceCode = BuildAttributesSourceCode(argument.Attributes);

            if (string.IsNullOrEmpty(attributesSourceCode) is false)
            {
                lineBuilder = lineBuilder.Append(attributesSourceCode).Append(' ');
            }

            var line = lineBuilder.Append(argument.TypeDisplayName).Append(' ').Append(argument.ArgumentName);

            var finalSign = (i < arguments.Length - 1) ? ',' : ')';
            line = line.Append(finalSign);

            builder.AppendCodeLine(line.ToString());
        }

        return builder;

        static int InnerGetOrderNumber(FunctionArgumentMetadata argumentMetadata)
            =>
            argumentMetadata.OrderNumber;

        string BuildAttributesSourceCode(IReadOnlyList<FunctionAttributeMetadata> attributes)
        {
            if (attributes.Any() is false)
            {
                return string.Empty;
            }

            var lineBuilder = new StringBuilder().Append('[');

            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];

                builder = builder.AddUsings(attribute.Namespaces);
                var attributeSourceCode = attribute.BuildSourceCode();

                lineBuilder = lineBuilder.Append(attributeSourceCode);
                if (i < attributes.Count - 1)
                {
                    lineBuilder = lineBuilder.Append(", ");
                }
            }

            return lineBuilder.Append(']').ToString();
        }
    }

    private static SourceBuilder AppendAzureFunctionBody(
        this SourceBuilder builder, FunctionProviderMetadata provider, HandlerResolverMetadata resolver)
    {
        builder = builder.AddUsings(
                resolver.HandlerType.AllNamespaces)
            .AddUsings(
                resolver.InputType.AllNamespaces)
            .AddUsings(
                resolver.OutputType.AllNamespaces);

        var resolverLineBuilder = new StringBuilder(
                $"{provider.ProviderType.DisplayedTypeName}.{resolver.ResolverMethodName}(");

        var resolverArguments = resolver.FunctionSpecificData.Arguments.Where(IsResolverArgument).OrderBy(GetResolverOrderNumber).ToArray();
        if (resolverArguments.Any() is false)
        {
            resolverLineBuilder = resolverLineBuilder.Append(')');
            builder = builder.AppendCodeLine(resolverLineBuilder.ToString());
        }
        else
        {
            builder = builder.AppendCodeLine(resolverLineBuilder.ToString()).BeginArguments();
            var resolverArgumentsLineBuilder = new StringBuilder();

            for (var i = 0; i < resolverArguments.Length; i++)
            {
                var resolverArgument = resolverArguments[i];
                resolverArgumentsLineBuilder = resolverArgumentsLineBuilder.Append(resolverArgument.ArgumentName);

                if (i < resolverArguments.Length - 1)
                {
                    resolverArgumentsLineBuilder = resolverArgumentsLineBuilder.Append(", ");
                }
            }

            resolverArgumentsLineBuilder = resolverArgumentsLineBuilder.Append(')');
            builder.AppendCodeLine(resolverArgumentsLineBuilder.ToString()).EndArguments();
        }

        var extensionsMethodName = resolver.FunctionSpecificData.ExtensionsMethodName;
        var handlerTypeName = resolver.HandlerType.DisplayedTypeName;
        var inputTypeName = resolver.InputType.DisplayedTypeName;
        var outputTypeName = resolver.OutputType.DisplayedTypeName;

        var extensionsMethodLineBuilder = new StringBuilder(
            $".{extensionsMethodName}<{handlerTypeName}, {inputTypeName}, {outputTypeName}>(");

        var extensionArguments = resolver.FunctionSpecificData.Arguments.Where(IsExtensionArgument).OrderBy(GetExtensionOrderNumber).ToArray();
        if (extensionArguments.Any() is false)
        {
            extensionsMethodLineBuilder = extensionsMethodLineBuilder.AppendLine(");");
            return builder.AppendCodeLine(extensionsMethodLineBuilder.ToString());
        }

        builder = builder.AppendCodeLine(extensionsMethodLineBuilder.ToString()).BeginArguments();

        var extensionLineBuilder = new StringBuilder();
        for (var i = 0; i < extensionArguments.Length; i++)
        {
            var extensionArgument = extensionArguments[i];
            extensionLineBuilder = extensionLineBuilder.Append(extensionArgument.ArgumentName);

            if (IsJsonRequestData(extensionArgument) && string.IsNullOrEmpty(resolver.JsonRootPath) is false)
            {
                var rootPathSourceCode = resolver.JsonRootPath.AsStringSourceCodeOrStringEmpty();
                extensionLineBuilder = extensionLineBuilder.Append(".GetProperty(").Append(rootPathSourceCode).Append(')');
            }

            if (i < extensionArguments.Length - 1)
            {
                extensionLineBuilder = extensionLineBuilder.Append(", ");
            }
        }

        extensionLineBuilder = extensionLineBuilder.Append(')').Append(';');
        return builder.AppendCodeLine(extensionLineBuilder.ToString()).EndArguments();

        static bool IsResolverArgument(FunctionArgumentMetadata argumentMetadata)
            =>
            argumentMetadata.ResolverMethodArgumentOrder is not null;

        static int GetResolverOrderNumber(FunctionArgumentMetadata argumentMetadata)
            =>
            argumentMetadata.ResolverMethodArgumentOrder.GetValueOrDefault();

        static bool IsExtensionArgument(FunctionArgumentMetadata argumentMetadata)
            =>
            argumentMetadata.ExtensionMethodArgumentOrder is not null;

        static int GetExtensionOrderNumber(FunctionArgumentMetadata argumentMetadata)
            =>
            argumentMetadata.ExtensionMethodArgumentOrder.GetValueOrDefault();

        static bool IsJsonRequestData(FunctionArgumentMetadata argumentMetadata)
            =>
            string.Equals("JsonElement", argumentMetadata.TypeDisplayName, StringComparison.Ordinal) &&
            string.Equals("requestData", argumentMetadata.ArgumentName, StringComparison.Ordinal);
    }

    private static string BuildSourceCode(this FunctionAttributeMetadata functionAttribute)
    {
        var lineBuilder = new StringBuilder();
        var typeDisplayName = functionAttribute.TypeDisplayName;

        if (typeDisplayName.EndsWith(Attribute))
        {
            typeDisplayName = typeDisplayName.Substring(0, typeDisplayName.Length - Attribute.Length);
        }

        lineBuilder = lineBuilder.Append(typeDisplayName);
        if (functionAttribute.ConstructorArgumentSourceCodes.Any() is false && functionAttribute.PropertySourceCodes.Any() is false)
        {
            return lineBuilder.ToString();
        }

        lineBuilder = lineBuilder.Append('(');
        for (var i = 0; i < functionAttribute.ConstructorArgumentSourceCodes.Count; i++)
        {
            lineBuilder = lineBuilder.Append(functionAttribute.ConstructorArgumentSourceCodes[i]);

            if (i < functionAttribute.ConstructorArgumentSourceCodes.Count - 1)
            {
                lineBuilder = lineBuilder.Append(", ");
            }
        }

        if (functionAttribute.ConstructorArgumentSourceCodes.Any() && functionAttribute.PropertySourceCodes.Any())
        {
            lineBuilder = lineBuilder.Append(", ");
        }

        for (var i = 0; i < functionAttribute.PropertySourceCodes.Count; i++)
        {
            var property = functionAttribute.PropertySourceCodes[i];
            lineBuilder = lineBuilder.Append(property.Key).Append(" = ").Append(property.Value);

            if (i < functionAttribute.PropertySourceCodes.Count - 1)
            {
                lineBuilder = lineBuilder.Append(", ");
            }
        }

        return lineBuilder.Append(')').ToString();
    }
}