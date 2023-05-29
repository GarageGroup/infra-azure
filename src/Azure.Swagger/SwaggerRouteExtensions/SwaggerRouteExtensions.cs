using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageGroup.Infra;

public static partial class SwaggerRouteExtensions
{
    private static string InnerGetRoutePrefix(this FunctionContext context)
        =>
        context.InstanceServices.GetService<IConfiguration>()?["extensions:http:routePrefix"] ?? "api";
}