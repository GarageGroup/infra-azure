using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace GarageGroup.Infra;

public static class FunctionHost
{
    public static IHostBuilder CreateFunctionsWorkerBuilderStandard(
        bool useHostConfiguration = false,
        Action<IFunctionsWorkerApplicationBuilder>? configure = null)
        =>
        new HostBuilder().InternalConfigureFunctionsWorkerStandard(useHostConfiguration, configure);
}