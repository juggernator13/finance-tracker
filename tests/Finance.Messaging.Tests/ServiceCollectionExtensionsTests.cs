using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Finance.Messaging.Tests;

public class PingHandler : IMessageHandler<Ping>
{
    public Task HandleAsync(Ping message, MessageContext context) => Task.CompletedTask;
}

public class ServiceCollectionExtensionsTests
{
    private static IConfiguration CreateConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMq:Host"] = "myhost",
                ["RabbitMq:User"] = "myuser",
                ["RabbitMq:Password"] = "mypass"
            })
            .Build();
    }

    [Fact]
    public void AddRabbitMq_ReadsSettingsFromConfig()
    {
        var services = new ServiceCollection();
        services.AddRabbitMq(CreateConfig());
        using var provider = services.BuildServiceProvider();

        var settings = provider.GetRequiredService<RabbitMqSettings>();

        Assert.Equal("myhost", settings.Host);
        Assert.Equal("myuser", settings.User);
        Assert.Equal("mypass", settings.Password);
    }

    [Fact]
    public void AddRabbitMq_RegistersMessageBus()
    {
        var services = new ServiceCollection();
        services.AddRabbitMq(CreateConfig());
        using var provider = services.BuildServiceProvider();

        var bus = provider.GetRequiredService<IMessageBus>();

        Assert.IsType<RabbitMqMessageBus>(bus);
    }

    [Fact]
    public void AddRabbitMq_BusAndConnectionAreSingletons()
    {
        var services = new ServiceCollection();
        services.AddRabbitMq(CreateConfig());
        using var provider = services.BuildServiceProvider();

        Assert.Same(provider.GetRequiredService<IMessageBus>(), provider.GetRequiredService<IMessageBus>());
        Assert.Same(provider.GetRequiredService<RabbitMqConnection>(), provider.GetRequiredService<RabbitMqConnection>());
    }

    [Fact]
    public void AddMessageHandler_RegistersHandlerAndConsumerService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRabbitMq(CreateConfig());
        services.AddMessageHandler<Ping, PingHandler>("test");
        using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<Ping>>();
        var hosted = provider.GetServices<IHostedService>().ToList();

        Assert.IsType<PingHandler>(handler);
        Assert.Single(hosted);
        Assert.IsType<RabbitMqConsumerService<Ping>>(hosted[0]);
    }
}