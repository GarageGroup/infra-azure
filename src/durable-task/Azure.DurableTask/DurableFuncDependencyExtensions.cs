using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class DurableFuncDependencyExtensions
{
    public static Task<TOut> RunOrchestrationFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency,
        TaskOrchestrationContext orchestrationContext,
        FunctionContext functionContext,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(orchestrationContext);
        ArgumentNullException.ThrowIfNull(functionContext);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        return dependency.Resolve(functionContext.InstanceServices).InternalInvokeOrchestrationFunctionAsync<THandler, TIn, TOut>(
            orchestrationContext, functionContext, cancellationToken);
    }

    public static Task<TOut> RunEntityFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency,
        TaskEntityDispatcher dispatcher,
        FunctionContext functionContext,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(functionContext);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        return dependency.Resolve(functionContext.InstanceServices).InternalInvokeEntityFunctionAsync<THandler, TIn, TOut>(
            dispatcher, functionContext, cancellationToken);
    }
}