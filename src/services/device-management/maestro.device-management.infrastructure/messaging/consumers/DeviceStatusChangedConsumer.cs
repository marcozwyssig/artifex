using MassTransit;
using Maestro.DeviceManagement.Domain.Events;
using Maestro.Shared.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace Maestro.DeviceManagement.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer for DeviceStatusChangedEvent
/// Implements both MassTransit IConsumer (for MassTransit) and IMessageConsumer (for abstraction)
/// </summary>
public class DeviceStatusChangedConsumer :
    IConsumer<DeviceStatusChangedEvent>,
    IMessageConsumer<DeviceStatusChangedEvent>
{
    private readonly ILogger<DeviceStatusChangedConsumer> _logger;

    public DeviceStatusChangedConsumer(ILogger<DeviceStatusChangedConsumer> logger)
    {
        _logger = logger;
    }

    // MassTransit interface
    public Task Consume(ConsumeContext<DeviceStatusChangedEvent> context)
    {
        return ConsumeAsync(context.Message, context.CancellationToken);
    }

    // Our abstraction interface
    public async Task ConsumeAsync(DeviceStatusChangedEvent message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing DeviceStatusChangedEvent for device {DeviceId} - Status: {Status}",
            message.DeviceId,
            message.NewStatus);

        // TODO: Add your event handling logic here
        // Examples:
        // - Alert on device going offline
        // - Update dashboard metrics
        // - Trigger health check workflow

        await Task.CompletedTask;
    }
}
