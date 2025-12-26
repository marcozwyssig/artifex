using Artifex.Shared.Domain;

namespace Artifex.Shared.Infrastructure.Messaging;

/// <summary>
/// Abstraction over message bus implementation (MassTransit, in-memory, etc.)
/// Follows Dependency Inversion Principle - domain depends on abstraction, not concrete implementation
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes a single event to the message bus
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    /// <summary>
    /// Publishes multiple events as a batch
    /// </summary>
    Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    /// <summary>
    /// Sends a command to a specific consumer
    /// </summary>
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class;

    /// <summary>
    /// Sends a command to a specific endpoint
    /// </summary>
    Task SendAsync<TCommand>(Uri destinationAddress, TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class;
}
