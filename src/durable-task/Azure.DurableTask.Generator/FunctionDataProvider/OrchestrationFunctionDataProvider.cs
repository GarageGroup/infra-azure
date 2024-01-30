using System;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed class OrchestrationFunctionDataProvider : IFunctionDataProvider
{
    public HandlerFunctionMetadata? GetFunctionMetadata(AttributeData functionAttribute, FunctionDataContext context)
    {
        if (functionAttribute.AttributeClass?.IsType("GarageGroup.Infra", "OrchestrationFunctionAttribute") is not true)
        {
            return null;
        }

        return new(
            namespaces:
            [
                "System.Threading",
                "System.Threading.Tasks",
                "Microsoft.DurableTask"
            ],
            responseTypeDisplayName: string.Equals("Unit", context.OutputType.DisplayedTypeName, StringComparison.Ordinal) switch
            {
                true => "Task",
                _ => $"Task<{context.OutputType.DisplayedTypeName}>"
            },
            extensionsMethodName: "RunOrchestrationFunctionAsync",
            arguments:
            [
                new(
                    namespaces: default,
                    typeDisplayName: "TaskOrchestrationContext",
                    argumentName: "orchestrationContext",
                    orderNumber: int.MinValue,
                    extensionMethodArgumentOrder: int.MinValue,
                    resolverMethodArgumentOrder: null,
                    attributes:
                    [
                        new(
                            namespaces: default,
                            typeDisplayName: "OrchestrationTrigger",
                            constructorArgumentSourceCodes: default,
                            propertySourceCodes: default),
                    ]),
                new(
                    namespaces: default,
                    typeDisplayName: "FunctionContext",
                    argumentName: "functionContext",
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