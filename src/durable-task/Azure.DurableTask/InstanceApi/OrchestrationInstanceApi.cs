using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace GarageGroup.Infra;

internal sealed partial class OrchestrationInstanceApi : IOrchestrationInstanceApi
{
    private readonly DurableTaskClient durableTaskClient;

    internal OrchestrationInstanceApi(DurableTaskClient durableTaskClient)
        =>
        this.durableTaskClient = durableTaskClient;

    private OrchestrationInstanceId GetInstanceId([AllowNull] string id)
        =>
        new(durableTaskClient, id);

    private sealed class OrchestrationInstanceId : IOrchestrationInstanceId, IHttpResponseProvider
    {
        private readonly DurableTaskClient client;

        public OrchestrationInstanceId(DurableTaskClient client, [AllowNull] string id)
        {
            this.client = client;
            Id = id.OrEmpty();
        }

        public string Id { get; }

        public HttpResponseData GetHttpResponse(HttpRequestData httpRequest)
            =>
            client.CreateCheckStatusResponse(httpRequest, Id);

        bool IEquatable<IOrchestrationInstanceId>.Equals(IOrchestrationInstanceId? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return IdComparer.Equals(Id, other.Id);
        }

        private static StringComparer IdComparer
            =>
            StringComparer.Ordinal;
    }
}