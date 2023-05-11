using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Functions.Worker.Http;
using PrimeFuncPack;

namespace GarageGroup.Infra.Endpoint;

public static class EndpointFuncDependencyExtensions
{
    public static Task<HttpResponseData> RunAzureFunctionAsync<TEndpoint>(
        this Dependency<TEndpoint> dependency, HttpRequestData request, CancellationToken cancellationToken = default)
        where TEndpoint : IEndpoint
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<HttpResponseData>(cancellationToken);
        }

        return dependency.Resolve(request.FunctionContext.InstanceServices).InvokeAzureFunctionAsync(request, cancellationToken);
    }

    public static async Task<HttpResponseData> InvokeAzureFunctionAsync(
        this IEndpoint endpoint, HttpRequestData request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(request.FunctionContext.CancellationToken, cancellationToken);

        var endpointRequest = CreateEndpointRequest(request);
        var endpointResponse = await endpoint.InvokeAsync(endpointRequest, tokenSource.Token).ConfigureAwait(false);

        var statusCode = (HttpStatusCode)endpointResponse.StatusCode;
        var response = request.CreateResponse(statusCode);

        if (endpointResponse.Body is not null)
        {
            response.Body = endpointResponse.Body;
        }

        foreach (var header in endpointResponse.Headers.Where(NotEmpty).GroupBy(GetLowerInvariantKey))
        {
            _ = response.Headers.TryAddWithoutValidation(header.Key, header.Select(GetValue));
        }

        return response;

        static bool NotEmpty(KeyValuePair<string, string?> pair)
            =>
            string.IsNullOrEmpty(pair.Value) is false;

        static string GetLowerInvariantKey(KeyValuePair<string, string?> pair)
            =>
            pair.Key.ToLowerInvariant();

        static string? GetValue(KeyValuePair<string, string?> pair)
            =>
            pair.Value;
    }

    private static EndpointRequest CreateEndpointRequest(HttpRequestData request)
    {
        return new(
            headers: request.Headers.Select(MapHeader).ToArray(),
            queryParameters: HttpUtility.ParseQueryString(request.Url.Query).AsEnumerable().ToArray(),
            routeValues: request.FunctionContext.BindingContext.BindingData?.Select(MapBindingData).ToArray(),
            user: new(request.Identities),
            body: request.Body);

        static KeyValuePair<string, string?> MapHeader(KeyValuePair<string, IEnumerable<string>> pair)
            =>
            new(pair.Key, string.Join(',', pair));

        static KeyValuePair<string, string?> MapBindingData(KeyValuePair<string, object?> pair)
            =>
            new(pair.Key, pair.Value?.ToString());
    }

    private static IEnumerable<KeyValuePair<string, string?>> AsEnumerable(this NameValueCollection collection)
    {
        foreach (var name in collection.AllKeys)
        {
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            yield return new(name, collection[name]);
        }
    }
}