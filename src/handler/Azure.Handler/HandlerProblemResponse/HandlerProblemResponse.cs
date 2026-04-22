using System;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace GarageGroup.Infra;

public static class HandlerProblemResponse
{
    public static HttpResponseData Build(
        HttpRequestData requestData, Failure<HandlerFailureCode> failure)
    {
        ArgumentNullException.ThrowIfNull(requestData);

        var statusCode = failure.FailureCode switch
        {
            HandlerFailureCode.Persistent => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var response = requestData.CreateResponse(statusCode);

        var problemDetails = new ProblemDetails
        {
            Type = failure.FailureCode switch
            {
                HandlerFailureCode.Persistent => "https://httpstatuses.com/400",
                _ => "https://httpstatuses.com/500"
            },
            Title = failure.FailureCode switch
            {
                HandlerFailureCode.Persistent => "Bad Request",
                _ => "Internal Server Error"
            },
            Status = (int)statusCode,
            Detail = failure.FailureMessage,
            Instance = requestData.Url.AbsolutePath
        };

        _ = response.Headers.TryAddWithoutValidation("Content-Type", "application/problem+json; charset=utf-8");
        response.WriteString(JsonSerializer.Serialize(problemDetails, JsonSerializerOptions.Web));

        return response;
    }

    private sealed record class ProblemDetails
    {
        public string? Type { get; init; }

        public string? Title { get; init; }

        public int Status { get; init; }

        public string? Detail { get; init; }

        public string? Instance { get; init; }
    }
}