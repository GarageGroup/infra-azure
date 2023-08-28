namespace GarageGroup.Infra;

public sealed record class OrchestrationActivityResult
{
    public bool IsSuccess { get; init; }

    public string? FailureMessage { get; init; }
}