using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static FunctionData? GetFunctionData(
        this Compilation compilation, CancellationToken cancellationToken)
    {
        var attributeData = compilation.Assembly.GetAttributes().FirstOrDefault(IsRefreshableTokenCredentialAttribute);
        if (attributeData?.GetNamedArgumentValue<bool?>("IsDisabled") is true)
        {
            return null;
        }

        var scheduleExpression = attributeData?.GetConstructorArgumentValue<string>(0);
        if (string.IsNullOrWhiteSpace(scheduleExpression))
        {
            scheduleExpression = DefaultScheduleExpression;
        }

        return new(
            scheduleExpression: scheduleExpression!,
            useMonitor: attributeData?.GetNamedArgumentValue<bool?>("UseMonitor") ?? false,
            runOnStartup: attributeData?.GetNamedArgumentValue<bool?>("RunOnStartup") ?? false);
    }
}