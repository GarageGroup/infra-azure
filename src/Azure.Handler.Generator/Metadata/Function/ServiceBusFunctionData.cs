namespace GarageGroup.Infra;

internal sealed record class ServiceBusFunctionData : BaseFunctionData
{
    public ServiceBusFunctionData(string queueName, string? connection)
    {
        QueueName = queueName ?? string.Empty;
        Connection = connection;
    }

    public string QueueName { get; }

    public string? Connection { get; }
}