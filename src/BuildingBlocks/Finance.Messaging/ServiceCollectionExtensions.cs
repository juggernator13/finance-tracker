using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finance.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration config)
    {
        var settings = new RabbitMqSettings();
        config.GetSection("RabbitMq").Bind(settings);

        services.AddSingleton(settings);
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

        return services;
    }

    public static IServiceCollection AddMessageHandler<TMessage, THandler>(
        this IServiceCollection services, string serviceName)
        where THandler : class, IMessageHandler<TMessage>
    {
        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        services.AddHostedService(sp => new RabbitMqConsumerService<TMessage>(
            sp.GetRequiredService<RabbitMqConnection>(),
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<ILogger<RabbitMqConsumerService<TMessage>>>(),
            serviceName));

        return services;
    }
}