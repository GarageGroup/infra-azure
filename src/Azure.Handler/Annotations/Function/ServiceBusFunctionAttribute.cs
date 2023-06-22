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

    public string QueueName { get; }

    public string Connection { get; }
}