using System;
using Microsoft.Extensions.DependencyInjection;

namespace GarageGroup.Infra;

partial class ExtendedServiceProvider<TService>
{
    public object GetRequiredService(Type serviceType)
    {
        if (serviceType == typeof(TService))
        {
            return service;
        }

        return sourceServiceProvider.GetRequiredService(serviceType);
    }
}