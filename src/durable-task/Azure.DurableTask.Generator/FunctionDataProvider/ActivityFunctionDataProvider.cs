using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed class ActivityFunctionDataProvider : IFunctionDataProvider
{
    public HandlerFunctionMetadata? GetFunctionMetadata(AttributeData functionAttribute, FunctionDataContext context)
    {
        if (functionAttribute.AttributeClass?.IsType("GarageGroup.Infra", "ActivityFunctionAttribute") is not true)
        {
            return null;
        }

        return new(
            namespaces:
            [
                "System.Text.Json",
                "System.Threading",
                "System.Threading.Tasks"
            ],
            responseTypeDisplayName: $"Task<HandlerResultJson<{context.OutputType.DisplayedTypeName}>>",
            extensionsMethodName: "RunAzureFunctionAsync",
            arguments:
            [
                new(
                    namespaces: default,
                    typeDisplayName: "JsonElement",
                    argumentName: "requestData",
                    orderNumber: int.MinValue,
                    extensionMethodArgumentOrder: int.MinValue,
                    resolverMethodArgumentOrder: null,
                    attributes:
                    [
                        new(
                            namespaces: default,
                            typeDisplayName: "ActivityTrigger",
                            constructorArgumentSourceCodes: default,
                            propertySourceCodes: default),
                    ]),
                new(
                    namespaces: default,
                    typeDisplayName: "FunctionContext",
                    argumentName: "context",
                    orderNumber: int.MaxValue - 1,
                    extensionMethodArgumentOrder: int.MaxValue - 1,
                    resolverMethodArgumentOrder: null,
                    attributes: default),
                new(
                    namespaces: default,
                    typeDisplayName: "CancellationToken",
                    argumentName: "cancellationToken",
                    orderNumber: int.MaxValue,
                    extensionMethodArgumentOrder: int.MaxValue,
                    resolverMethodArgumentOrder: null,
                    attributes: default)
            ]);
    }
}