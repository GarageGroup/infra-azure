namespace GarageGroup.Infra;

internal sealed record class ServiceBusFunctionData : BaseFunctionData
{
    public ServiceBusFunctionData(string? queueName, string? connection)
    {
        QueueName = queueName;
        Connection = connection;
    }

    public ServiceBusFunctionData(string? topicName, string? subscriptionName, string? connection)
    {
        SubscriptionName = subscriptionName;
        TopicName = topicName;
        Connection = connection;
    }

    public string? QueueName { get; }

    public string? TopicName { get; }

    public string? SubscriptionName { get; }

    public string? Connection { get; }
}