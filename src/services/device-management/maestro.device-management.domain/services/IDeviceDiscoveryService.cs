using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Services;

/// <summary>
/// Domain service for device discovery operations
/// </summary>
public interface IDeviceDiscoveryService
{
    /// <summary>
    /// Discovers devices in a network range
    /// </summary>
    Task<Result<IReadOnlyCollection<DiscoveredDevice>>> DiscoverDevicesAsync(
        string networkCidr,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Identifies a specific device by probing it
    /// </summary>
    Task<Result<DiscoveredDevice>> IdentifyDeviceAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies if a device at the given IP is reachable
    /// </summary>
    Task<bool> IsDeviceReachableAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a discovered device (not yet registered)
/// </summary>
public class DiscoveredDevice
{
    public string IpAddress { get; init; } = string.Empty;
    public string? MacAddress { get; set; }
    public string? Hostname { get; set; }
    public string? Vendor { get; set; }
    public string? DeviceType { get; set; }
    public string? Model { get; set; }
    public string? SoftwareVersion { get; set; }
    public bool IsReachable { get; set; }
    public DateTime DiscoveredAt { get; init; }
    public Dictionary<string, string> AdditionalInfo { get; init; } = new();
}
