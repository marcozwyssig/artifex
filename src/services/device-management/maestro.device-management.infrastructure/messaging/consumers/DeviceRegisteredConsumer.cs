using MassTransit;
using Maestro.DeviceManagement.Domain.Events;
using Maestro.Shared.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace Maestro.DeviceManagement.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer for DeviceRegisteredEvent
/// Implements both MassTransit IConsumer (for MassTransit) and IMessageConsumer (for abstraction)
/// </summary>
public class DeviceRegisteredConsumer :
    IConsumer<DeviceRegisteredEvent>,
    IMessageConsumer<DeviceRegisteredEvent>
{
    private readonly ILogger<DeviceRegisteredConsumer> _logger;

    public DeviceRegisteredConsumer(ILogger<DeviceRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    // MassTransit interface
    public Task Consume(ConsumeContext<DeviceRegisteredEvent> context)
    {
        return ConsumeAsync(context.Message, context.CancellationToken);
    }

    // Our abstraction interface
    public async Task ConsumeAsync(DeviceRegisteredEvent message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing DeviceRegisteredEvent for device {DeviceId} ({Hostname})",
            message.DeviceId,
            message.Hostname);

        // TODO: Add your event handling logic here
        // Examples:
        // - Send notification
        // - Update monitoring system
        // - Trigger device configuration workflow
        // - Update search index

        await Task.CompletedTask;
    }
}
