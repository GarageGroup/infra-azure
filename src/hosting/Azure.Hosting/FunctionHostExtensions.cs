using System;
using System.IO;
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
        return hostBuilder.InternalConfigureFunctionsWorkerStandard(useHostConfiguration, configure);
    }

    internal static IHostBuilder InternalConfigureFunctionsWorkerStandard(
        this IHostBuilder hostBuilder,
        bool useHostConfiguration,
        Action<IFunctionsWorkerApplicationBuilder>? configure)
    {
        var builder = hostBuilder
            .ConfigureAppConfiguration(AddHostConfiguration)
            .ConfigureSocketsHttpHandlerProvider()
            .ConfigureServices(InnerConfigureServiceCollection);

        if (configure is not null)
        {
            builder = builder.ConfigureFunctionsWorkerDefaults(configure);
        }
        else
        {
            builder = builder.ConfigureFunctionsWorkerDefaults();
        }

        return builder;

        static void InnerConfigureServiceCollection(IServiceCollection services)
            =>
            services.AddApplicationInsightsTelemetryWorkerService().ConfigureFunctionsApplicationInsights().AddTokenCredentialStandardAsSingleton();

        void AddHostConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            var rootPath = context.HostingEnvironment.ContentRootPath;
            configurationBuilder.AddJsonFile(Path.Combine(rootPath, "appsettings.json"));

            if (useHostConfiguration)
            {
                configurationBuilder.AddJsonFile(Path.Combine(rootPath, "host.json"));
            }
        }
    }
}