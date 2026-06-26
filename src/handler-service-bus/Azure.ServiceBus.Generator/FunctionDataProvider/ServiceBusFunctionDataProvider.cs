using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

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
                "Azure.Messaging.ServiceBus",
                "System.Threading",
                "System.Threading.Tasks"
            ],
            responseTypeDisplayName: "Task",
            extensionsMethodName: "RunServiceBusFunctionAsync",
            arguments:
            [
                new(
                    namespaces: default,
                    typeDisplayName: "ServiceBusReceivedMessage",
                    argumentName: "message",
                    orderNumber: int.MinValue,
                    extensionMethodArgumentOrder: int.MinValue,
                    resolverMethodArgumentOrder: null,
                    attributes:
                    [
                        BuildServiceBusTriggerAttributeMetadata(functionAttribute)
                    ]),
                new(
                    namespaces: default,
                    typeDisplayName: "ServiceBusMessageActions",
                    argumentName: "messageActions",
                    orderNumber: int.MinValue + 1,
                    extensionMethodArgumentOrder: int.MinValue + 1,
                    resolverMethodArgumentOrder: null,
                    attributes: default),
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
            var queueName = serviceBusAttribute.ConstructorArguments[1].Value?.ToString();
            constructorArguments.Add(queueName.AsStringSourceCodeOr());
        }
        else if (argumentsLength is SubscriptionServiceBusConstructorArgumentCount)
        {
            var topicName = serviceBusAttribute.ConstructorArguments[1].Value?.ToString();
            constructorArguments.Add(topicName.AsStringSourceCodeOr());

            var subscriptionName = serviceBusAttribute.ConstructorArguments[2].Value?.ToString();
            constructorArguments.Add(subscriptionName.AsStringSourceCodeOr());
        }
        else
        {
            throw new InvalidOperationException(
                $"An unexpected ServiceBusFunctionAttribute constructor arguments length: {argumentsLength}");
        }

        var properties = new Dictionary<string, string>();
        var connection = serviceBusAttribute.ConstructorArguments[argumentsLength - 1].Value?.ToString();

        if (string.IsNullOrEmpty(connection) is false)
        {
            properties["Connection"] = connection.AsStringSourceCodeOr();
        }

        properties["AutoCompleteMessages"] = "false";

        return new(
            namespaces: default,
            typeDisplayName: "ServiceBusTrigger",
            constructorArgumentSourceCodes: constructorArguments,
            propertySourceCodes: properties.ToArray());
    }
}