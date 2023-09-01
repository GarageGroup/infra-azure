using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

partial class HandlerFuncExtensions
{
    public static Task<HandlerResultJson<TOut>> InvokeAzureFunctionAsync<THandler, TIn, TOut>(
        this THandler handler, JsonElement jsonData, FunctionContext context, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(context);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<HandlerResultJson<TOut>>(cancellationToken);
        }

        return handler.InternalInvokeAzureFunctionAsync<THandler, TIn, TOut>(jsonData, context, cancellationToken);
    }

    internal static async Task<HandlerResultJson<TOut>> InternalInvokeAzureFunctionAsync<THandler, TIn, TOut>(
        this THandler handler, JsonElement json, FunctionContext context, CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, cancellationToken);
        var result = await json.DeserializeOrFailure<TIn>().ForwardValueAsync(handler.HandleOrFailureAsync, tokenSource.Token).ConfigureAwait(false);

        return result.MapFailure(LogFailure).MapFailure(ThrowIfTransient);

        Failure<HandlerFailureCode> LogFailure(Failure<HandlerFailureCode> failure)
        {
            var action = failure.FailureCode is HandlerFailureCode.Transient ? "retried" : "removed";

            context.GetFunctionLogger().LogError(
                failure.SourceException,
                "Data will be {action}: {data}. Error: {error}", action, json, failure.FailureMessage);

            context.TrackHandlerFailure(failure, json.ToString());
            return failure;
        }

        static Failure<HandlerFailureCode> ThrowIfTransient(Failure<HandlerFailureCode> failure)
        {
            if (failure.FailureCode is HandlerFailureCode.Transient)
            {
                throw failure.ToException();
            }

            return failure;
        }
    }
}