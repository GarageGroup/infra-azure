using System;

namespace GarageGroup.Infra;

public static class DurableTaskServiceProvider
{
    internal static IServiceProvider InternalExtend<TService>(this IServiceProvider sourceServiceProvider, TService service)
        where TService : notnull
        =>
        new ExtendedServiceProvider<TService>(sourceServiceProvider, service);
}