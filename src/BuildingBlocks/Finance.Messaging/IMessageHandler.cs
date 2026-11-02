namespace Finance.Messaging;

public interface IMessageHandler<T>
{
    public Task HandleAsync(T message, MessageContext context);
}