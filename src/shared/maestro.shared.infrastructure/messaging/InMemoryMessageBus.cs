using Maestro.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Maestro.Shared.Infrastructure.Messaging;

/// <summary>
/// In-memory implementation of IMessageBus for development and testing
/// Messages are handled synchronously within the same process
/// </summary>
public class InMemoryMessageBus : IMessageBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryMessageBus> _logger;
    private readonly Dictionary<Type, List<Type>> _subscriptions = new();

    public InMemoryMessageBus(
        IServiceProvider serviceProvider,
        ILogger<InMemoryMessageBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        var eventType = @event.GetType();
        _logger.LogDebug("Publishing event {EventType} in-memory", eventType.Name);

        if (!_subscriptions.TryGetValue(eventType, out var consumerTypes))
        {
            _logger.LogDebug("No consumers registered for event {EventType}", eventType.Name);
            return;
        }

        using var scope = _serviceProvider.CreateScope();

        foreach (var consumerType in consumerTypes)
        {
            try
            {
                var consumer = scope.ServiceProvider.GetService(consumerType);
                if (consumer == null)
                {
                    _logger.LogWarning("Consumer {ConsumerType} not registered in DI container", consumerType.Name);
                    continue;
                }

                var consumeMethod = consumerType.GetMethod("ConsumeAsync");
                if (consumeMethod != null)
                {
                    var task = consumeMethod.Invoke(consumer, new object[] { @event, cancellationToken }) as Task;
                    if (task != null)
                    {
                        await task;
                    }
                }

                _logger.LogDebug("Event {EventType} consumed by {ConsumerType}", eventType.Name, consumerType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming event {EventType} with {ConsumerType}", eventType.Name, consumerType.Name);
            }
        }
    }

    public async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }

    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class
    {
        // For in-memory, send is same as publish
        var commandType = command.GetType();
        _logger.LogDebug("Sending command {CommandType} in-memory", commandType.Name);

        if (!_subscriptions.TryGetValue(commandType, out var consumerTypes))
        {
            _logger.LogWarning("No consumer registered for command {CommandType}", commandType.Name);
            return;
        }

        using var scope = _serviceProvider.CreateScope();

        // Commands should have exactly one consumer
        var consumerType = consumerTypes.FirstOrDefault();
        if (consumerType == null) return;

        var consumer = scope.ServiceProvider.GetService(consumerType);
        if (consumer == null)
        {
            _logger.LogWarning("Consumer {ConsumerType} not registered in DI container", consumerType.Name);
            return;
        }

        var consumeMethod = consumerType.GetMethod("ConsumeAsync");
        if (consumeMethod != null)
        {
            var task = consumeMethod.Invoke(consumer, new object[] { command, cancellationToken }) as Task;
            if (task != null)
            {
                await task;
            }
        }
    }

    public Task SendAsync<TCommand>(Uri destinationAddress, TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class
    {
        // For in-memory, ignore destination and use default routing
        return SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Registers a consumer for a specific message type
    /// Used internally by the configuration extensions
    /// </summary>
    public void Subscribe<TMessage, TConsumer>()
        where TMessage : class
        where TConsumer : IMessageConsumer<TMessage>
    {
        var messageType = typeof(TMessage);
        var consumerType = typeof(TConsumer);

        if (!_subscriptions.ContainsKey(messageType))
        {
            _subscriptions[messageType] = new List<Type>();
        }

        if (!_subscriptions[messageType].Contains(consumerType))
        {
            _subscriptions[messageType].Add(consumerType);
            _logger.LogInformation("Registered consumer {ConsumerType} for message {MessageType}", consumerType.Name, messageType.Name);
        }
    }
}
