using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Shared.Infrastructure.Messaging;

/// <summary>
/// Extension methods for configuring messaging services using Dependency Injection
/// Supports both MassTransit (production) and InMemory (development/testing)
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Adds messaging services to the DI container
    /// Uses configuration to determine which implementation to use
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="configureConsumers">Action to configure consumers</param>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IMessagingConfigurator>? configureConsumers = null)
    {
        var useInMemory = configuration.GetValue<bool>("Messaging:UseInMemory", false);

        if (useInMemory)
        {
            services.AddInMemoryMessaging(configureConsumers);
        }
        else
        {
            services.AddMassTransitMessaging(configuration, configureConsumers);
        }

        return services;
    }

    /// <summary>
    /// Adds MassTransit-based messaging (production)
    /// </summary>
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IMessagingConfigurator>? configureConsumers = null)
    {
        var configurator = new MassTransitMessagingConfigurator();
        configureConsumers?.Invoke(configurator);

        services.AddMassTransit(busConfig =>
        {
            // Register all consumers
            foreach (var consumerRegistration in configurator.GetConsumerRegistrations())
            {
                consumerRegistration.Invoke(busConfig);
            }

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMQHost = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
                var rabbitMQPort = configuration.GetValue<ushort>("RabbitMQ:Port", 5672);
                var rabbitMQUsername = configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
                var rabbitMQPassword = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";
                var rabbitMQVHost = configuration.GetValue<string>("RabbitMQ:VirtualHost") ?? "/";

                cfg.Host(rabbitMQHost, rabbitMQPort, rabbitMQVHost, h =>
                {
                    h.Username(rabbitMQUsername);
                    h.Password(rabbitMQPassword);
                });

                // Configure retry policy
                cfg.UseMessageRetry(retryConfig =>
                {
                    retryConfig.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromMinutes(5),
                        intervalDelta: TimeSpan.FromSeconds(2));
                });

                // Configure dead letter queue error handling
                cfg.UseInMemoryOutbox();

                // Auto-configure endpoints for all registered consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        // Register our abstraction
        services.AddScoped<IMessageBus, MassTransitMessageBus>();

        return services;
    }

    /// <summary>
    /// Adds in-memory messaging (development/testing)
    /// </summary>
    public static IServiceCollection AddInMemoryMessaging(
        this IServiceCollection services,
        Action<IMessagingConfigurator>? configureConsumers = null)
    {
        var configurator = new InMemoryMessagingConfigurator(services);
        configureConsumers?.Invoke(configurator);

        // Register the in-memory bus as singleton (to maintain subscriptions)
        services.AddSingleton<InMemoryMessageBus>(sp =>
        {
            var bus = new InMemoryMessageBus(
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<InMemoryMessageBus>>());

            // Register all subscriptions
            foreach (var subscription in configurator.GetSubscriptions())
            {
                subscription.Invoke(bus);
            }

            return bus;
        });

        services.AddScoped<IMessageBus>(sp => sp.GetRequiredService<InMemoryMessageBus>());

        return services;
    }
}

/// <summary>
/// Interface for configuring messaging consumers
/// </summary>
public interface IMessagingConfigurator
{
    /// <summary>
    /// Adds a consumer for a specific message type
    /// </summary>
    IMessagingConfigurator AddConsumer<TConsumer>()
        where TConsumer : class;

    /// <summary>
    /// Adds a consumer with configuration
    /// </summary>
    IMessagingConfigurator AddConsumer<TConsumer>(Action<TConsumer>? configure)
        where TConsumer : class;
}

/// <summary>
/// MassTransit-specific configurator
/// </summary>
internal class MassTransitMessagingConfigurator : IMessagingConfigurator
{
    private readonly List<Action<IBusRegistrationConfigurator>> _consumerRegistrations = new();

    public IMessagingConfigurator AddConsumer<TConsumer>() where TConsumer : class
    {
        _consumerRegistrations.Add(cfg => cfg.AddConsumer<TConsumer>());
        return this;
    }

    public IMessagingConfigurator AddConsumer<TConsumer>(Action<TConsumer>? configure) where TConsumer : class
    {
        _consumerRegistrations.Add(cfg => cfg.AddConsumer<TConsumer>());
        return this;
    }

    public IEnumerable<Action<IBusRegistrationConfigurator>> GetConsumerRegistrations()
        => _consumerRegistrations;
}

/// <summary>
/// In-memory-specific configurator
/// </summary>
internal class InMemoryMessagingConfigurator : IMessagingConfigurator
{
    private readonly IServiceCollection _services;
    private readonly List<Action<InMemoryMessageBus>> _subscriptions = new();

    public InMemoryMessagingConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public IMessagingConfigurator AddConsumer<TConsumer>() where TConsumer : class
    {
        // Register consumer in DI
        _services.AddScoped<TConsumer>();

        // Find the message type from IMessageConsumer<T>
        var messageType = typeof(TConsumer)
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageConsumer<>))
            ?.GetGenericArguments()
            .FirstOrDefault();

        if (messageType != null)
        {
            // Create subscription using reflection
            var subscribeMethod = typeof(InMemoryMessageBus)
                .GetMethod(nameof(InMemoryMessageBus.Subscribe))!
                .MakeGenericMethod(messageType, typeof(TConsumer));

            _subscriptions.Add(bus => subscribeMethod.Invoke(bus, null));
        }

        return this;
    }

    public IMessagingConfigurator AddConsumer<TConsumer>(Action<TConsumer>? configure) where TConsumer : class
    {
        return AddConsumer<TConsumer>();
    }

    public IEnumerable<Action<InMemoryMessageBus>> GetSubscriptions()
        => _subscriptions;
}
