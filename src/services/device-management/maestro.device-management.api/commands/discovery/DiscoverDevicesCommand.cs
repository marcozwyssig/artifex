using Maestro.DeviceManagement.Domain.Services;
using Maestro.Shared.Infrastructure.Commands;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Commands;

/// <summary>
/// Command to discover devices in a network range
/// </summary>
public class DiscoverDevicesCommand : ICommand<Result<DeviceDiscoveryResult>>
{
    public string NetworkCidr { get; init; } = string.Empty;
    public bool AutoRegister { get; init; } = false;
    public string? DefaultUsername { get; init; }
    public string? DefaultPassword { get; init; }
}

/// <summary>
/// Result of device discovery operation
/// </summary>
public class DeviceDiscoveryResult
{
    public int TotalScanned { get; init; }
    public int DevicesFound { get; init; }
    public int DevicesRegistered { get; init; }
    public IReadOnlyCollection<DiscoveredDevice> Devices { get; init; } = Array.Empty<DiscoveredDevice>();
    public DateTime DiscoveryStartedAt { get; init; }
    public DateTime DiscoveryCompletedAt { get; init; }
    public TimeSpan Duration => DiscoveryCompletedAt - DiscoveryStartedAt;
}
