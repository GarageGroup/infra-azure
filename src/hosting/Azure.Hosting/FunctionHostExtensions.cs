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

        var builder = hostBuilder.ConfigureSocketsHttpHandlerProvider().ConfigureServices(InnerConfigureServiceCollection);

        if (configure is not null)
        {
            builder = builder.ConfigureFunctionsWorkerDefaults(configure);
        }
        else
        {
            builder = builder.ConfigureFunctionsWorkerDefaults();
        }

        if (useHostConfiguration)
        {
            builder = builder.ConfigureAppConfiguration(AddHostConfiguration);
        }

        return builder;

        static void InnerConfigureServiceCollection(IServiceCollection services)
            =>
            services.AddApplicationInsightsTelemetryWorkerService().ConfigureFunctionsApplicationInsights().AddTokenCredentialStandardAsSingleton();

        static void AddHostConfiguration(HostBuilderContext _, IConfigurationBuilder configurationBuilder)
            =>
            configurationBuilder.AddJsonFile("host.json");
    }
}