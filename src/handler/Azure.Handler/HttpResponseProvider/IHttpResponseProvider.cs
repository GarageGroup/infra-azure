using Microsoft.Azure.Functions.Worker.Http;

namespace GarageGroup.Infra;

public interface IHttpResponseProvider
{
    HttpResponseData GetHttpResponse(HttpRequestData httpRequest);
}