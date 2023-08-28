using System;
using System.Text;
using Microsoft.Azure.Functions.Worker;

namespace GarageGroup.Infra;

partial class SwaggerRouteExtensions
{
    public static string GetRouteUrl(this FunctionContext context, string? route)
    {
        ArgumentNullException.ThrowIfNull(context);

        const char Slash = '/';
        var routePrefix = context.InnerGetRoutePrefix().Trim(Slash);

        var builder = new StringBuilder().Append(Slash).Append(routePrefix);
        if (string.IsNullOrWhiteSpace(route))
        {
            return builder.ToString();
        }

        if (string.IsNullOrEmpty(routePrefix) is false)
        {
            builder = builder.Append(Slash);
        }

        var trimmedRoute = route.TrimStart(Slash);
        return builder.Append(trimmedRoute).ToString();
    }
}