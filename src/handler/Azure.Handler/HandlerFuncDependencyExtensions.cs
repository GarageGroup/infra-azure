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
    public static Task<HandlerResultJson<TOut>> RunAzureFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency, JsonElement jsonData, FunctionContext context, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(context);

        return dependency.Resolve(context.InstanceServices).InternalInvokeAzureFunctionAsync<THandler, TIn, TOut>(
            jsonData, context, cancellationToken);
    }

    public static Task<HttpResponseData> RunHttpFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency, HttpRequestData request, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(request);

        return dependency.Resolve(request.FunctionContext.InstanceServices).InternalHttpFunctionAsync<THandler, TIn, TOut>(
            request: request,
            readInputFunc: default,
            createSuccessResponseFunc: default,
            createFailureResponseFunc: default,
            cancellationToken: cancellationToken);
    }

    public static Task<HttpResponseData> RunHttpFunctionAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency,
        HttpRequestData request,
        Func<HttpRequestData, string, Result<TIn?, Failure<HandlerFailureCode>>>? readInputFunc,
        Func<HttpRequestData, TOut, HttpResponseData>? createSuccessResponseFunc,
        Func<HttpRequestData, Failure<HandlerFailureCode>, HttpResponseData>? createFailureResponseFunc,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(request);

        return dependency.Resolve(request.FunctionContext.InstanceServices).InternalHttpFunctionAsync(
            request: request,
            readInputFunc: readInputFunc,
            createSuccessResponseFunc: createSuccessResponseFunc,
            createFailureResponseFunc: createFailureResponseFunc,
            cancellationToken: cancellationToken);
    }
}