using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting;

public static class FunctionHostExtensions
{
    public static IHostBuilder ConfigureFunctionsWorkerStandard(
        this IHostBuilder hostBuilder, bool useHostConfiguration = false)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        var builder = hostBuilder.ConfigureSocketsHttpHandlerProvider().ConfigureFunctionsWorkerDefaults(Configure);
        if (useHostConfiguration is false)
        {
            return builder;
        }

        return builder.ConfigureAppConfiguration(AddHostConfiguration);

        static void Configure(IFunctionsWorkerApplicationBuilder builder)
            =>
            builder.AddApplicationInsights().AddApplicationInsightsLogger();

        static void AddHostConfiguration(HostBuilderContext _, IConfigurationBuilder configurationBuilder)
            =>
            configurationBuilder.AddJsonFile("host.json");
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