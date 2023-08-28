using System;
using Microsoft.Extensions.DependencyInjection;

namespace GarageGroup.Infra;

internal sealed partial class ExtendedServiceProvider<TService> : IServiceProvider, ISupportRequiredService
    where TService : notnull
{
    private readonly IServiceProvider sourceServiceProvider;

    private readonly TService service;

    internal ExtendedServiceProvider(IServiceProvider sourceServiceProvider, TService service)
    {
        this.sourceServiceProvider = sourceServiceProvider;
        this.service = service;
    }
}