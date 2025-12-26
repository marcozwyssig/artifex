using Maestro.DeviceManagement.Domain.Services;
using Maestro.Shared.Api.Queries;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Queries;

/// <summary>
/// Query to identify a single device
/// </summary>
public class IdentifyDeviceQuery : IQuery<Result<DiscoveredDevice>>
{
    public string IpAddress { get; init; } = string.Empty;
}
