using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed class ServiceBusFunctionDataProvider : IFunctionDataProvider
{
    private const int QueueServiceBusConstructorArgumentCount = 3;

    private const int SubscriptionServiceBusConstructorArgumentCount = 4;

    public HandlerFunctionMetadata? GetFunctionMetadata(AttributeData functionAttribute, FunctionDataContext _)
    {
        if (functionAttribute.AttributeClass?.IsType("GarageGroup.Infra", "ServiceBusFunctionAttribute") is not true)
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
            responseTypeDisplayName: "Task",
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
                        BuildServiceBusTriggerAttributeMetadata(functionAttribute)
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

    private static FunctionAttributeMetadata BuildServiceBusTriggerAttributeMetadata(AttributeData serviceBusAttribute)
    {
        var constructorArguments = new List<string>();

        var argumentsLength = serviceBusAttribute.ConstructorArguments.Length;
        if (argumentsLength is QueueServiceBusConstructorArgumentCount)
        {
            var queueName = serviceBusAttribute.GetAttributeValue(1)?.ToString();
            constructorArguments.Add(queueName.AsStringSourceCodeOr());
        }
        else if (argumentsLength is SubscriptionServiceBusConstructorArgumentCount)
        {
            var topicName = serviceBusAttribute.GetAttributeValue(1)?.ToString();
            constructorArguments.Add(topicName.AsStringSourceCodeOr());

            var subscriptionName = serviceBusAttribute.GetAttributeValue(2)?.ToString();
            constructorArguments.Add(subscriptionName.AsStringSourceCodeOr());
        }
        else
        {
            throw new InvalidOperationException(
                $"An unexpected ServiceBusFunctionAttribute constructor arguments length: {argumentsLength}");
        }

        var properties = new Dictionary<string, string>();
        var connection = serviceBusAttribute.GetAttributeValue(serviceBusAttribute.ConstructorArguments.Length - 1)?.ToString();

        if (string.IsNullOrEmpty(connection) is false)
        {
            properties["Connection"] = connection.AsStringSourceCodeOr();
        }

        return new(
            namespaces: default,
            typeDisplayName: "ServiceBusTrigger",
            constructorArgumentSourceCodes: constructorArguments,
            propertySourceCodes: properties.ToArray());
    }
}