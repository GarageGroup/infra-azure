using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed class EntityFunctionDataProvider : IFunctionDataProvider
{
    public HandlerFunctionMetadata? GetFunctionMetadata(AttributeData functionAttribute, FunctionDataContext context)
    {
        if (functionAttribute.AttributeClass?.IsType("GarageGroup.Infra", "EntityFunctionAttribute") is not true)
        {
            return null;
        }

        var dispatcherAttributeProperties = new List<KeyValuePair<string, string>>();
        var entityName = functionAttribute.GetAttributePropertyValue("EntityName")?.ToString();

        if (string.IsNullOrEmpty(entityName) is false)
        {
            dispatcherAttributeProperties.Add(new("EntityName", entityName.AsStringSourceCodeOr()));
        }

        return new(
            namespaces: new[]
            {
                "System.Threading",
                "System.Threading.Tasks",
                "Microsoft.Azure.Functions.Worker"
            },
            responseTypeDisplayName: string.Equals("Unit", context.OutputType.DisplayedTypeName, StringComparison.Ordinal) switch
            {
                true => "Task",
                _ => $"Task<{context.OutputType.DisplayedTypeName}>"
            },
            extensionsMethodName: "RunEntityFunctionAsync",
            arguments: new FunctionArgumentMetadata[]
            {
                new(
                    namespaces: default,
                    typeDisplayName: "TaskEntityDispatcher",
                    argumentName: "dispatcher",
                    orderNumber: int.MinValue,
                    extensionMethodArgumentOrder: int.MinValue,
                    resolverMethodArgumentOrder: null,
                    attributes: new FunctionAttributeMetadata[]
                    {
                        new(
                            namespaces: default,
                            typeDisplayName: "EntityTrigger",
                            constructorArgumentSourceCodes: default,
                            propertySourceCodes: dispatcherAttributeProperties),
                    }),
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
            });
    }
}