using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceBusFunctionAttribute : HandlerFunctionAttribute
{
    public ServiceBusFunctionAttribute(string name, string queueName, string connection) : base(name)
    {
        QueueName = queueName ?? string.Empty;
        Connection = connection ?? string.Empty;
    }

    public ServiceBusFunctionAttribute(string name, string topicName, string subscriptionName, string connection) : base(name)
    {
        TopicName = topicName ?? string.Empty;
        SubscriptionName = subscriptionName ?? string.Empty;
        Connection = connection ?? string.Empty;
    }

    public string? QueueName { get; }

    public string? TopicName { get; }

    public string? SubscriptionName { get; }

    public string Connection { get; }
}