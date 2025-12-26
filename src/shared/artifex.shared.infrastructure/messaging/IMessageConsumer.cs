namespace Artifex.Shared.Infrastructure.Messaging;

/// <summary>
/// Abstraction for message consumers
/// Implement this interface to handle messages from the message bus
/// </summary>
public interface IMessageConsumer<in TMessage>
    where TMessage : class
{
    /// <summary>
    /// Handles the consumed message
    /// </summary>
    Task ConsumeAsync(TMessage message, CancellationToken cancellationToken = default);
}
