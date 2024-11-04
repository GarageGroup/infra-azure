using System;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

public static class RefreshableTokenCredentialFunctionExtensions
{
    public static Task RefreshAzureTokensAsync(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.InstanceServices.GetService<TokenCredential>() is not ITokensRefreshSupplier tokenCredential)
        {
            return Task.CompletedTask;
        }

        context.GetLogger(context.FunctionDefinition.Name).LogInformation("Refresh Azure token credentials");
        return tokenCredential.RefreshTokensAsync(context.CancellationToken);
    }
}