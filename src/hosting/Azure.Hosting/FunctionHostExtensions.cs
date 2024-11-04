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
            .ConfigureAppConfiguration(ConfigureAzureAppConfiguration)
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
            services
                .AddApplicationInsightsTelemetryWorkerService()
                .ConfigureFunctionsApplicationInsights()
                .AddRefreshableTokenCredentialStandardAsSingleton();

        void AddHostConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            var rootPath = context.HostingEnvironment.ContentRootPath;
            configurationBuilder.AddJsonFile(Path.Combine(rootPath, "appsettings.json"));

            if (useHostConfiguration)
            {
                configurationBuilder.AddJsonFile(Path.Combine(rootPath, "host.json"));
            }
        }

        static void ConfigureAzureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            var connectionString = context.InnerGetConnectionString("AppConfig");
            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            builder.AddAzureAppConfiguration(connectionString);
        }
    }

    private static string? InnerGetConnectionString(this HostBuilderContext context, string name)
    {
        var connectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{name}");
        if (string.IsNullOrEmpty(connectionString) is false)
        {
            return connectionString;
        }

        connectionString = Environment.GetEnvironmentVariable($"ConnectionStrings:{name}");
        if (string.IsNullOrEmpty(connectionString) is false)
        {
            return connectionString;
        }

        return context.Configuration.GetConnectionString(name);
    }
}