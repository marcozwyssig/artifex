using MassTransit;
using Artifex.Shared.Domain;

namespace Artifex.Shared.Infrastructure.Messaging;

/// <summary>
/// MassTransit implementation of IMessageBus
/// Production implementation with RabbitMQ
/// </summary>
public class MassTransitMessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public MassTransitMessageBus(
        IPublishEndpoint publishEndpoint,
        ISendEndpointProvider sendEndpointProvider)
    {
        _publishEndpoint = publishEndpoint;
        _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }

    public async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        await _publishEndpoint.PublishBatch(events, cancellationToken);
    }

    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class
    {
        await _publishEndpoint.Publish(command, cancellationToken);
    }

    public async Task SendAsync<TCommand>(Uri destinationAddress, TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class
    {
        var endpoint = await _sendEndpointProvider.GetSendEndpoint(destinationAddress);
        await endpoint.Send(command, cancellationToken);
    }
}
