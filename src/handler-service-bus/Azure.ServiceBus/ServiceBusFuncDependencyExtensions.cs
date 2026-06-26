using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class ServiceBusFuncDependencyExtensions
{
    public static Task RunServiceBusFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency,
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        FunctionContext context,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(messageActions);
        ArgumentNullException.ThrowIfNull(context);

        return dependency.Resolve(context.InstanceServices).InternalRunServiceBusFunctionAsync<THandler, TIn, TOut>(
            message, messageActions, context, cancellationToken);
    }
}