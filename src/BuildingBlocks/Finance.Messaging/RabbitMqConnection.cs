using RabbitMQ.Client;

namespace Finance.Messaging;

public class RabbitMqConnection(RabbitMqSettings settings)
{
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IConnection> GetAsync()
    {
        if (_connection != null)
        {
            return _connection;
        }

        await _lock.WaitAsync();

        try
        {
            if (_connection != null)
            {
                return _connection;
            }

            var factory = new ConnectionFactory { HostName = settings.Host, UserName = settings.User, Password = settings.Password };

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync();
                    return _connection;
                }
                catch (Exception) when (attempt < 10)
                {
                    await Task.Delay(3000);
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}