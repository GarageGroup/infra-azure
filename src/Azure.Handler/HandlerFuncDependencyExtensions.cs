using System;
using System.Text.Json;
using System.Threading.Tasks;
using GGroupp.Infra;
using Microsoft.Azure.Functions.Worker;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HandlerFuncDependencyExtensions
{
    public static Task RunAzureFunctionAsync<THandler, THandlerData>(
        this Dependency<THandler> dependency, JsonElement jsonData, FunctionContext context)
        where THandler : IHandler<THandlerData>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(context);

        return dependency.Resolve(context.InstanceServices).InternalInvokeAzureFunctionAsync<THandler, THandlerData>(jsonData, context);
    }
}