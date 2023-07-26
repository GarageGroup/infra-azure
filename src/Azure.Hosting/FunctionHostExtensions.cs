using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class FunctionHostExtensions
{
    public static IHostBuilder ConfigureFunctionsWorkerStandard(
        this IHostBuilder hostBuilder,
        bool useHostConfiguration = false,
        Action<IFunctionsWorkerApplicationBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        var builder = hostBuilder.ConfigureSocketsHttpHandlerProvider().ConfigureServices(InnerConfigureApplicationInsights);

        if (useHostConfiguration)
        {
            builder = builder.ConfigureAppConfiguration(AddHostConfiguration);
        }

        if (configure is not null)
        {
            builder = builder.ConfigureFunctionsWorkerDefaults(configure);
        }

        return builder;

        static void InnerConfigureApplicationInsights(IServiceCollection services)
            =>
            services.ConfigureFunctionsApplicationInsights();

        static void AddHostConfiguration(HostBuilderContext _, IConfigurationBuilder configurationBuilder)
            =>
            configurationBuilder.AddJsonFile("host.json");
    }
}