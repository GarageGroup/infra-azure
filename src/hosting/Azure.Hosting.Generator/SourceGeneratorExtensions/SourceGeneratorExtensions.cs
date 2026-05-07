using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string DefaultScheduleExpression = "0 */30 * * * *";

    private static bool IsRefreshableTokenCredentialAttribute(AttributeData attributeData)
        =>
        attributeData.AttributeClass?.IsType("GarageGroup.Infra", "RefreshableTokenCredentialAttribute") is true;
}