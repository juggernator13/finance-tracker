namespace Finance.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string? correlationId = null);
}