using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HandlerFuncDependencyExtensions
{
    public static Task RunAzureFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency, JsonElement jsonData, FunctionContext context, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(context);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        return dependency.Resolve(context.InstanceServices).InternalInvokeAzureFunctionAsync<THandler, TIn, TOut>(jsonData, context, cancellationToken);
    }

    public static Task<HttpResponseData> RunHttpFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency, HttpRequestData request, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<HttpResponseData>(cancellationToken);
        }

        return dependency.Resolve(request.FunctionContext.InstanceServices).InternalHttpFunctionAsync<THandler, TIn, TOut>(request, cancellationToken);
    }
}