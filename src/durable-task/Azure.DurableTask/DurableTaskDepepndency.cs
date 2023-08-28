using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageGroup.Infra;

public static class DurableTaskDepepndency
{
    private const int BackoffCoefficientDefault = -1;

    private static OrchestrationActivityApiOption ResolveActivityApiOption(IServiceProvider serviceProvider, string sectionName)
    {
        var section = serviceProvider.GetRequiredService<IConfiguration>().GetSection(sectionName);

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