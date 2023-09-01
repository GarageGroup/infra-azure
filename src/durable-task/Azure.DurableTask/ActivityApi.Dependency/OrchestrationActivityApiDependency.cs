using System;
using Microsoft.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class OrchestrationActivityApiDependency
{
    private const int BackoffCoefficientDefault = 1;

    public static Dependency<IOrchestrationActivityApi> UseOrchestrationActivityApi(
        this Dependency<TaskOrchestrationContext> dependency, string sectionName = "OrchestrationActivity")
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.With(InnerResolveOption).Fold<IOrchestrationActivityApi>(ResolveApi);

        OrchestrationActivityApiOption? InnerResolveOption(IServiceProvider serviceProvider)
            =>
            serviceProvider.ResolveActivityApiOption(sectionName.OrEmpty());
    }

    public static Dependency<IOrchestrationActivityApi> UseOrchestrationActivityApi(
        this Dependency<TaskOrchestrationContext, OrchestrationActivityApiOption> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Fold<IOrchestrationActivityApi>(ResolveApi);
    }

    private static OrchestrationActivityApi ResolveApi(
        TaskOrchestrationContext context, OrchestrationActivityApiOption? option)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(context, option);
    }

    private static OrchestrationActivityApiOption? ResolveActivityApiOption(
        this IServiceProvider serviceProvider, string sectionName)
    {
        var section = serviceProvider.GetRequiredService<IConfiguration>().GetSection(sectionName);
        if (section.Exists() is false)
        {
            return null;
        }

        return new(
            maxNumberOfAttempts: section.GetInt32Value("MaxNumberOfAttempts"),
            firstRetryInterval: section.GetInt32Value("FirstRetryIntervalInSeconds").ToTimeSpanFromSeconds(),
            backoffCoefficient: section.GetNullableDoubleValue("BackoffCoefficient") ?? BackoffCoefficientDefault)
        {
            MaxRetryInterval = section.GetNullableInt32Value("MaxRetryIntervalInSeconds")?.ToTimeSpanFromSeconds(),
            RetryTimeout = section.GetNullableInt32Value("RetryTimeoutInSeconds")?.ToTimeSpanFromSeconds()
        };
    }

    private static int GetInt32Value(this IConfigurationSection section, string key)
        =>
        section.GetNullableInt32Value(key) ?? throw UnspecifiedValueException($"{section.Path}:{key}");

    private static int? GetNullableInt32Value(this IConfigurationSection section, string key)
    {
        var value = section[key];

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (int.TryParse(value, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"Configuration '{section.Path}:{key}' must be a valid Int32 value");
    }

    private static double? GetNullableDoubleValue(this IConfigurationSection section, string key)
    {
        var value = section[key];

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (double.TryParse(value, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"Configuration '{key}' must be a valid Double value");
    }

    private static TimeSpan ToTimeSpanFromSeconds(this int seconds)
        =>
        TimeSpan.FromSeconds(seconds);

    private static InvalidOperationException UnspecifiedValueException(string key)
        =>
        new($"Configuration '{key}' value must be specified");
}