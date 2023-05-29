using System;
using Microsoft.Azure.Functions.Worker;

namespace GarageGroup.Infra;

partial class SwaggerRouteExtensions
{
    public static string GetRoutePrefix(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.InnerGetRoutePrefix();
    }
}