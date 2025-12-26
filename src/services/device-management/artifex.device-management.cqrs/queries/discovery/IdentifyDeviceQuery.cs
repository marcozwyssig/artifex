using Artifex.DeviceManagement.Domain.Services;
using Artifex.Shared.Cqrs.Queries;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Cqrs.Queries;

/// <summary>
/// Query to identify a single device
/// </summary>
public class IdentifyDeviceQuery : IQuery<Result<DiscoveredDevice>>
{
    public string IpAddress { get; init; } = string.Empty;
}
