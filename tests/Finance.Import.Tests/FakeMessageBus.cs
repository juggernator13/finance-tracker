using Finance.Messaging;

namespace Finance.Import.Tests;

public class FakeMessageBus : IMessageBus
{
    public List<object> Published { get; } = new();
    public Exception? Error { get; set; }

    public Task PublishAsync<T>(T message, string? correlationId = null)
    {
        if (Error != null) throw Error;
        Published.Add(message!);
        return Task.CompletedTask;
    }
}