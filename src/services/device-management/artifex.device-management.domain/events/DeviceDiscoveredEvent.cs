using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Events;

/// <summary>
/// Event raised when a new device is discovered on the network
/// </summary>
public class DeviceDiscoveredEvent : DomainEvent
{
    public string IpAddress { get; }
    public string? MacAddress { get; }
    public string? Hostname { get; }
    public string? DetectedVendor { get; }
    public string? DetectedType { get; }
    public bool IsReachable { get; }

    public DeviceDiscoveredEvent(
        string ipAddress,
        string? macAddress = null,
        string? hostname = null,
        string? detectedVendor = null,
        string? detectedType = null,
        bool isReachable = true)
    {
        IpAddress = ipAddress;
        MacAddress = macAddress;
        Hostname = hostname;
        DetectedVendor = detectedVendor;
        DetectedType = detectedType;
        IsReachable = isReachable;
    }
}
