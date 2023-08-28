namespace GarageGroup.Infra;

public sealed record class OrchestrationActivityResult<T>
{
    public T? Value { get; init; }

    public bool IsSuccess { get; init; }

    public string? FailureMessage { get; init; }
}