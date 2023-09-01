using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace GarageGroup.Infra;

public static class TelemetryExtensions
{
    public static void TrackHandlerFailure(
        this FunctionContext context, Failure<HandlerFailureCode> failure, [AllowNull] string requestData = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.InternalTrackHandlerFailure(failure, requestData ?? string.Empty);
    }

    internal static void InternalTrackHandlerFailure(
        this FunctionContext context, Failure<HandlerFailureCode> failure, string? requestData)
    {
        var properties = new Dictionary<string, string>
        {
            ["function"] = context.FunctionDefinition.Name,
            ["message"] = failure.FailureMessage
        };

        if (string.IsNullOrEmpty(requestData) is false)
        {
            properties["data"] = requestData;
        }

        if (failure.SourceException is not null)
        {
            properties["errorMessage"] = failure.SourceException.Message ?? string.Empty;
            properties["errorType"] = failure.SourceException.GetType().FullName ?? string.Empty;
            properties["stackTrace"] = failure.SourceException.StackTrace ?? string.Empty;
        }

        var eventName = failure.FailureCode is HandlerFailureCode.Transient ? "TransientFailure" : "PersistentFailure";
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(eventName, properties);
    }
}