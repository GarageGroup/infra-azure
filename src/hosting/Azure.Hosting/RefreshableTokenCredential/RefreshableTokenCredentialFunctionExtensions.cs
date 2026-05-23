using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

public static class RefreshableTokenCredentialFunctionExtensions
{
    private const int LevelOfParallelism = 4;

    public static async Task RefreshAzureTokensAsync(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var credentials = context.InstanceServices.GetServices<ITokensRefreshSupplier>().ToArray();
        if (credentials.Length is 0)
        {
            return;
        }

        var logger = context.GetLogger(context.FunctionDefinition.Name);
        logger.LogInformation("Refresh {Count} Azure token credentials.", credentials.Length);

        var exceptions = new ConcurrentBag<Exception>();

        foreach (var credentialChunk in credentials.Chunk(LevelOfParallelism))
        {
            await Task.WhenAll(credentialChunk.Select(InnerRefreshTokensAsync));
        }

        if (exceptions.IsEmpty)
        {
            return;
        }

        if (exceptions.Count is 1 && exceptions.TryPeek(out var exception))
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
            return;
        }

        throw new AggregateException("An error occurred while refreshing Azure token credentials.", exceptions);

        async Task InnerRefreshTokensAsync(ITokensRefreshSupplier tokenCredential)
        {
            try
            {
                await tokenCredential.RefreshTokensAsync(context.CancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                exceptions.Add(ex);
            }
        }
    }
}