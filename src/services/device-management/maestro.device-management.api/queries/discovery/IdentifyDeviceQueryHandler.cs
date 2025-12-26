using Maestro.DeviceManagement.Domain.Services;
using Maestro.Shared.Api.Queries;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Queries;

/// <summary>
/// Handler for IdentifyDeviceQuery
/// </summary>
public class IdentifyDeviceQueryHandler : IQueryHandler<IdentifyDeviceQuery, Result<DiscoveredDevice>>
{
    private readonly IDeviceDiscoveryService _discoveryService;

    public IdentifyDeviceQueryHandler(IDeviceDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    public async Task<Result<DiscoveredDevice>> HandleAsync(
        IdentifyDeviceQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.IpAddress))
        {
            return Result.Failure<DiscoveredDevice>("IP address cannot be empty");
        }

        return await _discoveryService.IdentifyDeviceAsync(query.IpAddress, cancellationToken);
    }
}
