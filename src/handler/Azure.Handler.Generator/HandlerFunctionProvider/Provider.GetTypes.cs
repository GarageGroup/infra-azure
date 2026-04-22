using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class HandlerFunctionProvider
{
    public IReadOnlyCollection<FunctionProviderMetadata> GetFunctionProviderTypes(Compilation compilation, System.Threading.CancellationToken cancellationToken)
    {
        var visitor = new ExportedTypesCollector(cancellationToken);
        visitor.VisitNamespace(compilation.GlobalNamespace);

        return visitor.GetNonPrivateTypes().Select(GetFunctionMetadata).NotNull().ToArray();
    }

    private FunctionProviderMetadata? GetFunctionMetadata(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeArguments.Any())
        {
            return null;
        }

        var resolverTypes = typeSymbol.GetMembers().OfType<IMethodSymbol>().SelectMany(GetResolversMetadata).ToArray();
        if (resolverTypes.Any() is false)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + typeNameSuffix,
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: resolverTypes);
    }

    private IReadOnlyList<HandlerResolverMetadata> GetResolversMetadata(IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Where(IsFunctionAttribute).Select(InnerGetResolverMetadata).NotNull().ToArray();

        static bool IsFunctionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.BaseType?.IsType(DefaultNamespace, "HandlerFunctionAttribute") is true;

        HandlerResolverMetadata? InnerGetResolverMetadata(AttributeData functionAttribute)
            =>
            GetResolverMetadata(methodSymbol, functionAttribute);
    }

    private HandlerResolverMetadata? GetResolverMetadata(IMethodSymbol methodSymbol, AttributeData functionAttribute)
    {
        if (methodSymbol.IsStatic is false)
        {
            throw methodSymbol.CreateInvalidMethodException("must be static");
        }

        if (methodSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            throw methodSymbol.CreateInvalidMethodException("must be public or internal");
        }

        if (methodSymbol.TypeParameters.Any())
        {
            throw methodSymbol.CreateInvalidMethodException("must have no generic arguments");
        }

        var handlerType = GetResolvedHandlerType(methodSymbol.ReturnType, methodSymbol);
        var handlerInterface = IsHandlerType(handlerType) ? handlerType : handlerType?.AllInterfaces.FirstOrDefault(IsHandlerType);

        var inputType = handlerInterface?.TypeArguments.FirstOrDefault();
        var outputType = handlerInterface?.TypeArguments.ElementAtOrDefault(1);

        if (handlerType is null || inputType is null || outputType is null)
        {
            throw methodSymbol.CreateInvalidMethodException($"must resolve a type that implements {DefaultNamespace}.IHandler<TIn, TOut>");
        }

        var functionDataContext = new FunctionDataContext(
            handlerType: handlerType.GetDisplayedData(),
            inputType: inputType.GetDisplayedData(),
            outputType: outputType.GetDisplayedData());

        var functionSpecificData = GetFunctionSpecificData(methodSymbol, functionAttribute, functionDataContext);
        if (functionSpecificData is null)
        {
            return null;
        }

        var functionName = functionAttribute.GetAttributeValue(0, "Name")?.ToString() ?? methodSymbol.Name;
        var readInputFunc = GetReadInputFuncOrDefault(methodSymbol, functionAttribute, inputType);
        var createSuccessResponseFunc = GetCreateSuccessResponseFuncOrDefault(methodSymbol, functionAttribute, outputType);
        var createFailureResponseFunc = GetCreateFailureResponseFuncOrDefault(methodSymbol, functionAttribute);

        return new(
            handlerType: functionDataContext.HandlerType,
            inputType: functionDataContext.InputType,
            outputType: functionDataContext.OutputType,
            resolverMethodName: methodSymbol.Name,
            functionMethodName: functionName + "Async",
            functionName: functionName,
            readInputFuncName: readInputFunc?.Name,
            readInputFuncType: readInputFunc?.Type,
            createSuccessResponseFuncName: createSuccessResponseFunc?.Name,
            createSuccessResponseFuncType: createSuccessResponseFunc?.Type,
            createFailureResponseFuncName: createFailureResponseFunc?.Name,
            createFailureResponseFuncType: createFailureResponseFunc?.Type,
            jsonRootPath: GetJsonRootPath(inputType),
            functionSpecificData: functionSpecificData);

        static bool IsHandlerType(INamedTypeSymbol? typeSymbol)
            =>
            typeSymbol?.IsType(DefaultNamespace, "IHandler") is true && typeSymbol.TypeParameters.Length is 2;
    }

    private static INamedTypeSymbol GetResolvedHandlerType(ITypeSymbol dependencyType, IMethodSymbol methodSymbol)
    {
        if (dependencyType is not INamedTypeSymbol namedDependencyType)
        {
            throw methodSymbol.CreateInvalidMethodException(
                "return type must be a named type with public instance Resolve(System.IServiceProvider) method");
        }

        var resolveMethod = EnumerateResolveMethods(namedDependencyType).FirstOrDefault(IsResolveContractMatch)
         ?? throw methodSymbol.CreateInvalidMethodException(
                "return type must contain a public instance Resolve(System.IServiceProvider) method without generic arguments");

        if (resolveMethod.ReturnType is not INamedTypeSymbol handlerType)
        {
            throw methodSymbol.CreateInvalidMethodException(
                "Resolve(System.IServiceProvider) must return a named type");
        }

        return handlerType;

        static IEnumerable<IMethodSymbol> EnumerateResolveMethods(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind is TypeKind.Interface)
            {
                foreach (var methodSymbol in typeSymbol.GetMembers("Resolve").OfType<IMethodSymbol>())
                {
                    yield return methodSymbol;
                }

                foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
                {
                    foreach (var methodSymbol in interfaceSymbol.GetMembers("Resolve").OfType<IMethodSymbol>())
                    {
                        yield return methodSymbol;
                    }
                }

                yield break;
            }

            for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
            {
                foreach (var methodSymbol in currentType.GetMembers("Resolve").OfType<IMethodSymbol>())
                {
                    yield return methodSymbol;
                }
            }
        }

        static bool IsResolveContractMatch(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind is not MethodKind.Ordinary)
            {
                return false;
            }

            if (methodSymbol.IsStatic)
            {
                return false;
            }

            if (methodSymbol.DeclaredAccessibility is not Accessibility.Public)
            {
                return false;
            }

            if (methodSymbol.TypeParameters.Any())
            {
                return false;
            }

            if (methodSymbol.Parameters.Length is not 1)
            {
                return false;
            }

            var parameterSymbol = methodSymbol.Parameters[0];
            if (parameterSymbol.RefKind is not RefKind.None)
            {
                return false;
            }

            if (parameterSymbol.Type.IsType("System", "IServiceProvider") is false)
            {
                return false;
            }

            return methodSymbol.ReturnsVoid is false;
        }
    }

    private static ReferencedFuncData? GetReadInputFuncOrDefault(
        IMethodSymbol resolverMethod,
        AttributeData functionAttribute,
        ITypeSymbol inputType)
    {
        if (functionAttribute.AttributeClass?.IsType(DefaultNamespace, "HttpFunctionAttribute") is not true)
        {
            return null;
        }

        var readInputFuncName = functionAttribute.GetAttributePropertyValue("ReadInputFunc")?.ToString();
        if (string.IsNullOrEmpty(readInputFuncName))
        {
            return null;
        }

        var methodName = readInputFuncName!;
        var selector = GetFuncTypeSelectorOrDefault(resolverMethod, functionAttribute, "ReadInputFuncType");

        var contractMethod = selector.Type
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(InnerIsValidReadInputContractMatch);

        if (contractMethod is null)
        {
            throw resolverMethod.CreateInvalidMethodException(
                $"has an invalid ReadInputFunc '{methodName}', expected static method with signature Func<HttpRequestData, string, Result<{inputType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}?, Failure<HandlerFailureCode>>>");
        }

        return new(
            type: selector.Type.GetDisplayedData(),
            name: methodName);

        bool InnerIsValidReadInputContractMatch(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind is not MethodKind.Ordinary || methodSymbol.IsStatic is false)
            {
                return false;
            }

            if (IsExpectedAccessibility(methodSymbol.DeclaredAccessibility, selector.IsExplicitType) is false)
            {
                return false;
            }

            if (methodSymbol.TypeParameters.Any() || methodSymbol.Parameters.Length is not 2)
            {
                return false;
            }

            if (methodSymbol.Parameters[0].RefKind is not RefKind.None ||
                methodSymbol.Parameters[1].RefKind is not RefKind.None)
            {
                return false;
            }

            if (methodSymbol.Parameters[0].Type.IsType("Microsoft.Azure.Functions.Worker.Http", "HttpRequestData") is false)
            {
                return false;
            }

            if (methodSymbol.Parameters[1].Type.SpecialType is not SpecialType.System_String)
            {
                return false;
            }

            if (methodSymbol.ReturnType is not INamedTypeSymbol returnType ||
                returnType.TypeArguments.Length is not 2)
            {
                return false;
            }

            var returnInputType = returnType.TypeArguments[0];
            if (IsReadInputTypeMismatch(returnInputType, inputType))
            {
                return false;
            }

            var returnFailureType = returnType.TypeArguments[1] as INamedTypeSymbol;
            if (returnFailureType is null ||
                returnFailureType.TypeArguments.Length is not 1)
            {
                return false;
            }

            return returnFailureType.Name.Equals("Failure", StringComparison.Ordinal) &&
                returnFailureType.TypeArguments[0].Name.Equals("HandlerFailureCode", StringComparison.Ordinal);
        }

        static bool IsReadInputTypeMismatch(ITypeSymbol actualReturnInputType, ITypeSymbol handlerInputType)
        {
            if (handlerInputType.IsReferenceType)
            {
                if (SymbolEqualityComparer.Default.Equals(actualReturnInputType, handlerInputType) is false)
                {
                    return true;
                }

                return actualReturnInputType.NullableAnnotation is not NullableAnnotation.Annotated;
            }

            if (actualReturnInputType is INamedTypeSymbol namedReturnInputType &&
                namedReturnInputType.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T &&
                namedReturnInputType.TypeArguments.Length is 1 &&
                SymbolEqualityComparer.Default.Equals(namedReturnInputType.TypeArguments[0], handlerInputType))
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(actualReturnInputType, handlerInputType);
        }
    }

    private static ReferencedFuncData? GetCreateSuccessResponseFuncOrDefault(
        IMethodSymbol resolverMethod,
        AttributeData functionAttribute,
        ITypeSymbol outputType)
    {
        if (functionAttribute.AttributeClass?.IsType(DefaultNamespace, "HttpFunctionAttribute") is not true)
        {
            return null;
        }

        var createSuccessResponseFuncName = functionAttribute.GetAttributePropertyValue("CreateSuccessResponseFunc")?.ToString();
        if (string.IsNullOrEmpty(createSuccessResponseFuncName))
        {
            return null;
        }

        var methodName = createSuccessResponseFuncName!;
        var selector = GetFuncTypeSelectorOrDefault(resolverMethod, functionAttribute, "CreateSuccessResponseFuncType");
        var contractMethod = selector.Type
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(InnerIsValidCreateSuccessResponseContractMatch);

        if (contractMethod is null)
        {
            throw resolverMethod.CreateInvalidMethodException(
                $"has an invalid CreateSuccessResponseFunc '{methodName}', expected static method with signature Func<HttpRequestData, {outputType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}, HttpResponseData>");
        }

        return new(
            type: selector.Type.GetDisplayedData(),
            name: methodName);

        bool InnerIsValidCreateSuccessResponseContractMatch(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind is not MethodKind.Ordinary || methodSymbol.IsStatic is false)
            {
                return false;
            }

            if (IsExpectedAccessibility(methodSymbol.DeclaredAccessibility, selector.IsExplicitType) is false)
            {
                return false;
            }

            if (methodSymbol.TypeParameters.Any() || methodSymbol.Parameters.Length is not 2)
            {
                return false;
            }

            if (methodSymbol.Parameters[0].RefKind is not RefKind.None ||
                methodSymbol.Parameters[1].RefKind is not RefKind.None)
            {
                return false;
            }

            if (methodSymbol.Parameters[0].Type.IsType("Microsoft.Azure.Functions.Worker.Http", "HttpRequestData") is false)
            {
                return false;
            }

            if (SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[1].Type, outputType) is false)
            {
                return false;
            }

            return methodSymbol.ReturnType.IsType("Microsoft.Azure.Functions.Worker.Http", "HttpResponseData");
        }
    }

    private static ReferencedFuncData? GetCreateFailureResponseFuncOrDefault(
        IMethodSymbol resolverMethod,
        AttributeData functionAttribute)
    {
        if (functionAttribute.AttributeClass?.IsType(DefaultNamespace, "HttpFunctionAttribute") is not true)
        {
            return null;
        }

        var createFailureResponseFuncName = functionAttribute.GetAttributePropertyValue("CreateFailureResponseFunc")?.ToString();
        if (string.IsNullOrEmpty(createFailureResponseFuncName))
        {
            return null;
        }

        var methodName = createFailureResponseFuncName!;
        var selector = GetFuncTypeSelectorOrDefault(resolverMethod, functionAttribute, "CreateFailureResponseFuncType");
        var contractMethod = selector.Type
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(IsValidCreateFailureResponseContractMatch);

        if (contractMethod is null)
        {
            throw resolverMethod.CreateInvalidMethodException(
                "has an invalid CreateFailureResponseFunc '" + methodName +
                "', expected static method with signature Func<HttpRequestData, Failure<HandlerFailureCode>, HttpResponseData>");
        }

        return new(
            type: selector.Type.GetDisplayedData(),
            name: methodName);

        bool IsValidCreateFailureResponseContractMatch(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind is not MethodKind.Ordinary || methodSymbol.IsStatic is false)
            {
                return false;
            }

            if (IsExpectedAccessibility(methodSymbol.DeclaredAccessibility, selector.IsExplicitType) is false)
            {
                return false;
            }

            if (methodSymbol.TypeParameters.Any() || methodSymbol.Parameters.Length is not 2)
            {
                return false;
            }

            if (methodSymbol.Parameters[0].RefKind is not RefKind.None ||
                methodSymbol.Parameters[1].RefKind is not RefKind.None)
            {
                return false;
            }

            if (methodSymbol.Parameters[0].Type.IsType("Microsoft.Azure.Functions.Worker.Http", "HttpRequestData") is false)
            {
                return false;
            }

            var secondParameterType = methodSymbol.Parameters[1].Type as INamedTypeSymbol;
            if (secondParameterType is null ||
                secondParameterType.Name.Equals("Failure", StringComparison.Ordinal) is false ||
                secondParameterType.TypeArguments.Length is not 1 ||
                secondParameterType.TypeArguments[0].Name.Equals("HandlerFailureCode", StringComparison.Ordinal) is false)
            {
                return false;
            }

            return methodSymbol.ReturnType.IsType("Microsoft.Azure.Functions.Worker.Http", "HttpResponseData");
        }
    }

    private static FuncTypeSelector GetFuncTypeSelectorOrDefault(
        IMethodSymbol resolverMethod,
        AttributeData functionAttribute,
        string funcTypePropertyName)
    {
        var explicitType = functionAttribute.NamedArguments
            .Where(static a => a.Value.Kind is TypedConstantKind.Type)
            .Where(a => string.Equals(a.Key, funcTypePropertyName, StringComparison.Ordinal))
            .Select(a => a.Value.Value as ITypeSymbol)
            .OfType<INamedTypeSymbol>()
            .FirstOrDefault();

        if (explicitType is not null)
        {
            return new(
                type: explicitType,
                isExplicitType: true);
        }

        return new(
            type: resolverMethod.ContainingType,
            isExplicitType: false);
    }

    private static bool IsExpectedAccessibility(Accessibility accessibility, bool isExplicitType)
        =>
        isExplicitType
            ? accessibility is Accessibility.Public
            : accessibility is Accessibility.Public or Accessibility.Internal;

    private readonly struct FuncTypeSelector
    {
        internal FuncTypeSelector(INamedTypeSymbol type, bool isExplicitType)
        {
            Type = type;
            IsExplicitType = isExplicitType;
        }

        internal INamedTypeSymbol Type { get; }

        internal bool IsExplicitType { get; }
    }

    private readonly struct ReferencedFuncData
    {
        internal ReferencedFuncData(DisplayedTypeData type, string name)
        {
            Type = type;
            Name = name;
        }

        internal DisplayedTypeData Type { get; }

        internal string Name { get; }
    }

    private HandlerFunctionMetadata? GetFunctionSpecificData(
        IMethodSymbol methodSymbol, AttributeData functionAttribute, FunctionDataContext context)
    {
        var functionMetadata = functionDataProviders.Select(InnerGetFunctionMetadata).NotNull().FirstOrDefault();

        if (functionMetadata is null)
        {
            return null;
        }

        if (methodSymbol.Parameters.Any() is false)
        {
            return functionMetadata;
        }

        var defaultArguments = functionMetadata.Arguments.ToDictionary(GetTypeName);
        var parameterArguments = methodSymbol.Parameters.Select(GetArgumentMetadata).ToArray();

        var arguments = new List<FunctionArgumentMetadata>(parameterArguments.Length);
        foreach (var parameterArgument in parameterArguments)
        {
            if (defaultArguments.TryGetValue(parameterArgument.TypeDisplayName, out var defaultArgument) is false)
            {
                arguments.Add(parameterArgument);
                continue;
            }

            var argument = new FunctionArgumentMetadata(
                namespaces: parameterArgument.Namespaces,
                typeDisplayName: parameterArgument.TypeDisplayName,
                argumentName: defaultArgument.ArgumentName,
                orderNumber: defaultArgument.OrderNumber,
                extensionMethodArgumentOrder: defaultArgument.ExtensionMethodArgumentOrder,
                resolverMethodArgumentOrder: parameterArgument.ResolverMethodArgumentOrder,
                attributes: parameterArgument.Attributes);

            arguments.Add(argument);
            defaultArguments.Remove(parameterArgument.TypeDisplayName);
        }

        return new(
            namespaces: functionMetadata.Namespaces,
            responseTypeDisplayName: functionMetadata.ResponseTypeDisplayName,
            extensionsMethodName: functionMetadata.ExtensionsMethodName,
            arguments: arguments.Concat(defaultArguments.Values).ToArray());

        HandlerFunctionMetadata? InnerGetFunctionMetadata(IFunctionDataProvider provider)
            =>
            provider.GetFunctionMetadata(functionAttribute, context);

        static string GetTypeName(FunctionArgumentMetadata argument)
            =>
            argument.TypeDisplayName;
    }

    private static FunctionArgumentMetadata GetArgumentMetadata(IParameterSymbol parameter, int order)
    {
        var type = parameter.Type.GetDisplayedData();

        return new(
            namespaces: type.AllNamespaces.ToArray(),
            typeDisplayName: type.DisplayedTypeName,
            argumentName: parameter.Name,
            orderNumber: order,
            extensionMethodArgumentOrder: null,
            resolverMethodArgumentOrder: order,
            attributes: parameter.GetAttributes().Select(GetAttributeMetadata).NotNull().ToArray());
    }

    private static FunctionAttributeMetadata? GetAttributeMetadata(AttributeData attribute)
    {
        var type = attribute.AttributeClass?.GetDisplayedData();
        if (type is null)
        {
            return null;
        }

        var namespaces = type.AllNamespaces.ToList();

        return new(
            namespaces: namespaces,
            typeDisplayName: type.DisplayedTypeName,
            constructorArgumentSourceCodes: attribute.ConstructorArguments.Select(BuildArgumentSourceCode).ToArray(),
            propertySourceCodes: attribute.NamedArguments.Select(BuildPropertySourceCode).ToArray());

        KeyValuePair<string, string> BuildPropertySourceCode(KeyValuePair<string, TypedConstant> namedArgument)
            =>
            new(namedArgument.Key, BuildArgumentSourceCode(namedArgument.Value));

        string BuildArgumentSourceCode(TypedConstant argument)
        {
            if (argument.Value is null)
            {
                return "null";
            }

            if (argument.Value is string stringValue)
            {
                return stringValue.AsStringSourceCodeOr();
            }

            if (argument.Type?.GetEnumUnderlyingTypeOrDefault() is not null)
            {
                var enumType = argument.Type.GetDisplayedData();
                namespaces.AddRange(enumType.AllNamespaces);
                return $"({enumType.DisplayedTypeName}){argument.Value}";
            }

            return argument.Value.ToString();
        }
    }

    private static string? GetJsonRootPath(ITypeSymbol eventDataType)
    {
        return eventDataType.GetAttributes().FirstOrDefault(IsHandlerDataJsonAttribute)?.GetAttributeValue(0)?.ToString();

        static bool IsHandlerDataJsonAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HandlerDataJsonAttribute") is true;
    }
}
