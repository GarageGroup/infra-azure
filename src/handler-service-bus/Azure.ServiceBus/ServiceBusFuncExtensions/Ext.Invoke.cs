using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

partial class ServiceBusFuncExtensions
{
    internal static async Task InternalRunServiceBusFunctionAsync<THandler, TIn, TOut>(
        this THandler handler,
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        FunctionContext context,
        CancellationToken cancellationToken)
        where THandler : IHandler<TIn, TOut>
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, cancellationToken);
        var token = tokenSource.Token;

        var result = await ParseBodyOrFailure<TIn>(message.Body).ForwardValueAsync(handler.HandleOrFailureAsync, token);
        if (result.IsSuccess)
        {
            await messageActions.CompleteMessageAsync(message, token);
            return;
        }

        var failure = result.FailureOrThrow();

        var action = failure.FailureCode is HandlerFailureCode.Transient ? "abandoned" : "dead-lettered";
        context.GetLogger(context.FunctionDefinition.Name).LogError(
            failure.SourceException,
            "A ServiceBus message will be {action}: {messageId}. Error: {error}",
            action, message.MessageId, failure.FailureMessage);

        context.TrackHandlerFailure(failure, message.MessageId);

        if (failure.FailureCode is HandlerFailureCode.Transient)
        {
            await messageActions.AbandonMessageAsync(message, cancellationToken: token);
        }
        else
        {
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: failure.FailureMessage,
                deadLetterErrorDescription: failure.SourceException?.Message,
                cancellationToken: token);
        }
    }

    private static Result<TIn?, Failure<HandlerFailureCode>> ParseBodyOrFailure<TIn>(BinaryData body)
    {
        try
        {
            return JsonSerializer.Deserialize<TIn>(body, JsonSerializerOptions.Web) ?? default;
        }
        catch (Exception exception)
        {
            return exception.ToFailure(
                HandlerFailureCode.Persistent, "An unexpected error occurred when the message body was being deserialized");
        }
    }
}