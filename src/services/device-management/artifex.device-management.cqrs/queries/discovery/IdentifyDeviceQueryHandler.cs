using Artifex.DeviceManagement.Domain.Services;
using Artifex.Shared.Cqrs.Queries;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Cqrs.Queries;

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
