using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Finance.Messaging;

public class RabbitMqConsumerService<TMessage>(
    RabbitMqConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumerService<TMessage>> logger,
    string serviceName) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var conn = await connection.GetAsync();
        await using var channel = await conn.CreateChannelAsync();

        string key = typeof(TMessage).Name;
        string queue = $"{serviceName}.{key}";

        await channel.ExchangeDeclareAsync("finance", ExchangeType.Direct, durable: true);
        await channel.ExchangeDeclareAsync("finance.dlx", ExchangeType.Fanout, durable: true);
        await channel.QueueDeclareAsync("finance.dead", durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync("finance.dead", "finance.dlx", "");

        var args = new Dictionary<string, object?> { ["x-dead-letter-exchange"] = "finance.dlx" };
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: args);
        await channel.QueueBindAsync(queue, "finance", key);

        await channel.BasicQosAsync(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) => await ProcessAsync(channel, ea);
        await channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task ProcessAsync(IChannel channel, BasicDeliverEventArgs ea)
    {
        TMessage message;
        try
        {
            message = MessageSerializer.Deserialize<TMessage>(ea.Body);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot read {Type}, sending to dead queue", typeof(TMessage).Name);
            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
            return;
        }

        var context = new MessageContext(ea.BasicProperties.MessageId ?? "", ea.BasicProperties.CorrelationId);

        bool done = false;
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
                await handler.HandleAsync(message, context);
                done = true;
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Attempt {Attempt} failed for {Type}", attempt, typeof(TMessage).Name);
                if (attempt < 3)
                {
                    await Task.Delay(1000 * attempt);
                }
            }
        }

        if (done)
        {
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        else
        {
            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }
}