using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed class EventGridFunctionDataProvider : IFunctionDataProvider
{
    public HandlerFunctionMetadata? GetFunctionMetadata(AttributeData functionAttribute, FunctionDataContext _)
    {
        if (functionAttribute.AttributeClass?.IsType("GarageGroup.Infra", "EventGridFunctionAttribute") is not true)
        {
            return null;
        }

        return new(
            namespaces: new[]
            {
                "System.Text.Json",
                "System.Threading",
                "System.Threading.Tasks"
            },
            responseTypeDisplayName: "Task",
            extensionsMethodName: "RunAzureFunctionAsync",
            arguments: new FunctionArgumentMetadata[]
            {
                new(
                    namespaces: default,
                    typeDisplayName: "JsonElement",
                    argumentName: "requestData",
                    orderNumber: int.MinValue,
                    extensionMethodArgumentOrder: int.MinValue,
                    resolverMethodArgumentOrder: null,
                    attributes: new FunctionAttributeMetadata[]
                    {
                        new(
                            namespaces: default,
                            typeDisplayName: "EventGridTrigger",
                            constructorArgumentSourceCodes: default,
                            propertySourceCodes: default),
                    }),
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
            });
    }
}