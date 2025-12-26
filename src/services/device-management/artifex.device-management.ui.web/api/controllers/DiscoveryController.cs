using Artifex.DeviceManagement.Application.Commands;
using Artifex.DeviceManagement.Application.Queries;
using Artifex.DeviceManagement.Domain.Services;
using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Api.Queries;
using Artifex.Shared.Domain;
using Artifex.Shared.Ui.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Artifex.DeviceManagement.Ui.Api.Controllers;

/// <summary>
/// API controller for device discovery operations
/// </summary>
[Route("api/discovery")]
public class DiscoveryController : BaseApiController
{
    private readonly ICommandHandler<DiscoverDevicesCommand, Result<DeviceDiscoveryResult>> _discoverDevicesHandler;
    private readonly IQueryHandler<IdentifyDeviceQuery, Result<DiscoveredDevice>> _identifyDeviceHandler;
    private readonly ILogger<DiscoveryController> _logger;

    public DiscoveryController(
        ICommandHandler<DiscoverDevicesCommand, Result<DeviceDiscoveryResult>> discoverDevicesHandler,
        IQueryHandler<IdentifyDeviceQuery, Result<DiscoveredDevice>> identifyDeviceHandler,
        ILogger<DiscoveryController> logger)
    {
        _discoverDevicesHandler = discoverDevicesHandler;
        _identifyDeviceHandler = identifyDeviceHandler;
        _logger = logger;
    }

    /// <summary>
    /// Discovers devices in a network range
    /// </summary>
    [HttpPost("discover")]
    [ProducesResponseType(typeof(DeviceDiscoveryResult), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<DeviceDiscoveryResult>> DiscoverDevices(
        [FromBody] DiscoverDevicesRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Discovery request received for network {NetworkCidr}", request.NetworkCidr);

        var command = new DiscoverDevicesCommand
        {
            NetworkCidr = request.NetworkCidr,
            AutoRegister = request.AutoRegister,
            DefaultUsername = request.DefaultUsername,
            DefaultPassword = request.DefaultPassword
        };

        var result = await _discoverDevicesHandler.HandleAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Identifies a single device by IP address
    /// </summary>
    [HttpGet("identify/{ipAddress}")]
    [ProducesResponseType(typeof(DiscoveredDevice), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<DiscoveredDevice>> IdentifyDevice(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Device identification request for {IpAddress}", ipAddress);

        var query = new IdentifyDeviceQuery { IpAddress = ipAddress };
        var result = await _identifyDeviceHandler.HandleAsync(query, cancellationToken);

        return HandleResult(result);
    }
}

/// <summary>
/// Request model for device discovery
/// </summary>
public class DiscoverDevicesRequest
{
    public string NetworkCidr { get; init; } = string.Empty;
    public bool AutoRegister { get; init; } = false;
    public string? DefaultUsername { get; init; }
    public string? DefaultPassword { get; init; }
}
