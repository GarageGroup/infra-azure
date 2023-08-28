using System;

namespace GarageGroup.Infra;

partial class ExtendedServiceProvider<TService>
{
    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(TService))
        {
            return service;
        }

        return sourceServiceProvider.GetService(serviceType);
    }
}