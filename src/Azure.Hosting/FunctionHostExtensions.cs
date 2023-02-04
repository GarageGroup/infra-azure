using System;
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Extensions.Hosting;

public static class FunctionHostExtensions
{
    public static IHostBuilder ConfigureFunctionsWorkerStandard(
        this IHostBuilder hostBuilder)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        return hostBuilder.ConfigureSocketsHttpHandlerProvider().ConfigureFunctionsWorkerDefaults(Configure);

        static void Configure(IFunctionsWorkerApplicationBuilder builder)
            =>
            builder.AddApplicationInsights().AddApplicationInsightsLogger();
    }

    public static IHostBuilder ConfigureFunctionsWorkerStandard(
        this IHostBuilder hostBuilder,
        Action<IFunctionsWorkerApplicationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        ArgumentNullException.ThrowIfNull(configure);

        return hostBuilder.ConfigureSocketsHttpHandlerProvider().ConfigureFunctionsWorkerDefaults(Configure);

        void Configure(IFunctionsWorkerApplicationBuilder builder)
        {
            configure.Invoke(builder);
            builder.AddApplicationInsights().AddApplicationInsightsLogger();
        }
    }
}