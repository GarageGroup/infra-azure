using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

public interface IFunctionDataProvider
{
    HandlerFunctionMetadata? GetFunctionMetadata(AttributeData functionAttribute, FunctionDataContext context);
}