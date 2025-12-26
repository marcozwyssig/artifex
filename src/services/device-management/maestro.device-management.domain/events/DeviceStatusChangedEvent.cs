using Maestro.DeviceManagement.Domain.Enums;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Events;

/// <summary>
/// Event raised when device status changes
/// </summary>
public class DeviceStatusChangedEvent : DomainEvent
{
    public Guid DeviceId { get; }
    public DeviceStatus OldStatus { get; }
    public DeviceStatus NewStatus { get; }
    public string? Reason { get; }

    public DeviceStatusChangedEvent(Guid deviceId, DeviceStatus oldStatus, DeviceStatus newStatus, string? reason = null)
    {
        DeviceId = deviceId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Reason = reason;
    }
}
