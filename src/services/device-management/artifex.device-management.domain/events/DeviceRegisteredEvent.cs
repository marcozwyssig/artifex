using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Events;

/// <summary>
/// Event raised when a new device is registered
/// </summary>
public class DeviceRegisteredEvent : DomainEvent
{
    public Guid DeviceId { get; }
    public string Hostname { get; }
    public string IpAddress { get; }
    public string DeviceType { get; }

    public DeviceRegisteredEvent(Guid deviceId, string hostname, string ipAddress, string deviceType)
    {
        DeviceId = deviceId;
        Hostname = hostname;
        IpAddress = ipAddress;
        DeviceType = deviceType;
    }
}
