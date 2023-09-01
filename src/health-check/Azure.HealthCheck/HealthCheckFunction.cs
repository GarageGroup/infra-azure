using System;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace GarageGroup.Infra;

public static class HealthCheckFunction
{
    public static HttpResponseData Run(HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var response = request.CreateResponse(HttpStatusCode.OK);

        response.Headers.Add("Content-Type", "application/json");
        response.WriteString("{\"status\": \"Healthy\"}");

        return response;
    }
}