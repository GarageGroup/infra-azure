using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace GarageGroup.Infra;

public static partial class HandlerFuncExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions;

    static HandlerFuncExtensions()
        =>
        SerializerOptions = new(JsonSerializerDefaults.Web);

    private static async Task<string> ReadAsStringAsync(this Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return await streamReader.ReadToEndAsync().ConfigureAwait(false) ?? string.Empty;
    }

    private static void TrackPersistentFailure(this FunctionContext context, string requestData, string message)
        =>
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "PersistentFailure",
            new Dictionary<string, string>
            {
                ["function"] = context.FunctionDefinition.Name,
                ["data"] = requestData,
                ["message"] = message
            });

    private static void TrackTransientFailure(this FunctionContext context, string requestData, string message)
        =>
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "TransientFailure",
            new Dictionary<string, string>
            {
                ["function"] = context.FunctionDefinition.Name,
                ["data"] = requestData,
                ["message"] = message
            });

    private static void TrackException(this FunctionContext context, string requestData, Exception exception)
        =>
        context.InstanceServices.GetService<TelemetryClient>()?.TrackEvent(
            "TransientFailure",
            new Dictionary<string, string>
            {
                ["function"] = context.FunctionDefinition.Name,
                ["data"] = requestData,
                ["message"] = exception.Message,
                ["errorType"] = exception.GetType().FullName ?? string.Empty,
                ["stackTrace"] = exception.StackTrace ?? string.Empty
            });
}