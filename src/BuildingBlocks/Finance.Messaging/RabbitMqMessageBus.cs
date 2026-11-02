using RabbitMQ.Client;

namespace Finance.Messaging;

public class RabbitMqMessageBus(RabbitMqConnection connection) : IMessageBus
{
    public async Task PublishAsync<T>(T message, string? correlationId = null)
    {
        var _connetion = await connection.GetAsync();
        await using var channel = await _connetion.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("finance", ExchangeType.Direct, durable: true);

        var props = new BasicProperties
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: "finance",
            routingKey: typeof(T).Name,
            mandatory: false,
            basicProperties: props,
            body: MessageSerializer.Serialize(message));
    }
}