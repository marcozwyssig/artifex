using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Events;

/// <summary>
/// Event raised when a device is deleted
/// </summary>
public class DeviceDeletedEvent : DomainEvent
{
    public Guid DeviceId { get; }
    public string Hostname { get; }
    public string IpAddress { get; }

    public DeviceDeletedEvent(Guid deviceId, string hostname, string ipAddress)
    {
        DeviceId = deviceId;
        Hostname = hostname;
        IpAddress = ipAddress;
    }
}
