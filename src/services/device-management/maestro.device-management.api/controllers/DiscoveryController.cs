using Maestro.DeviceManagement.Api.Services;
using Maestro.DeviceManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Maestro.DeviceManagement.Api.Controller;

/// <summary>
/// API controller for managing local network discovery
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiscoveryController : ControllerBase
{
    private readonly LocalNetworkDiscoveryService _discoveryService;
    private readonly DiscoveryOptions _options;
    private readonly ILogger<DiscoveryController> _logger;

    public DiscoveryController(
        LocalNetworkDiscoveryService discoveryService,
        IOptions<DiscoveryOptions> options,
        ILogger<DiscoveryController> logger)
    {
        _discoveryService = discoveryService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Triggers discovery for all configured network segments
    /// </summary>
    [HttpPost("discover-all")]
    [ProducesResponseType(typeof(DiscoveryResultResponse), 200)]
    public async Task<ActionResult<DiscoveryResultResponse>> DiscoverAll(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual discovery triggered for all segments");

        var segments = GetConfiguredSegments();
        var results = await _discoveryService.DiscoverAllSegmentsAsync(
            segments,
            _options.DefaultUsername,
            _options.DefaultPassword,
            cancellationToken);

        var response = new DiscoveryResultResponse
        {
            SegmentsScanned = results.Count,
            TotalDevicesFound = results.Values.Sum(),
            SegmentResults = results.Select(r => new SegmentResult
            {
                SegmentName = r.Key,
                DevicesFound = r.Value
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Triggers discovery for a specific network segment
    /// </summary>
    [HttpPost("discover-segment")]
    [ProducesResponseType(typeof(SegmentDiscoveryResponse), 200)]
    public async Task<ActionResult<SegmentDiscoveryResponse>> DiscoverSegment(
        [FromBody] DiscoverSegmentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual discovery triggered for segment {SegmentName}", request.SegmentName);

        var segmentConfig = _options.NetworkSegments.FirstOrDefault(s => s.Name == request.SegmentName);
        if (segmentConfig == null)
        {
            return NotFound(new { error = $"Segment '{request.SegmentName}' not found" });
        }

        var segmentResult = LocalNetworkSegment.Create(
            segmentConfig.NetworkCidr,
            segmentConfig.Name,
            segmentConfig.EnableAutoDiscovery,
            segmentConfig.DiscoveryIntervalMinutes);

        if (segmentResult.IsFailure)
        {
            return BadRequest(new { errors = segmentResult.Error });
        }

        var devicesFound = await _discoveryService.DiscoverSegmentAsync(
            segmentResult.Value!,
            _options.DefaultUsername,
            _options.DefaultPassword,
            cancellationToken);

        var response = new SegmentDiscoveryResponse
        {
            SegmentName = request.SegmentName,
            NetworkCidr = segmentConfig.NetworkCidr,
            DevicesFound = devicesFound,
            DiscoveredAt = DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// Identifies a specific device
    /// </summary>
    [HttpGet("identify/{ipAddress}")]
    [ProducesResponseType(typeof(DeviceIdentificationResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DeviceIdentificationResponse>> IdentifyDevice(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual device identification for {IpAddress}", ipAddress);

        var deviceInfo = await _discoveryService.IdentifyDeviceAsync(ipAddress, cancellationToken);

        if (deviceInfo == null)
        {
            return NotFound(new { error = $"Could not identify device at {ipAddress}" });
        }

        var response = new DeviceIdentificationResponse
        {
            IpAddress = deviceInfo.IpAddress,
            MacAddress = deviceInfo.MacAddress,
            Hostname = deviceInfo.Hostname,
            Vendor = deviceInfo.Vendor,
            DeviceType = deviceInfo.DeviceType,
            Model = deviceInfo.Model,
            IsReachable = deviceInfo.IsReachable
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets configured network segments
    /// </summary>
    [HttpGet("segments")]
    [ProducesResponseType(typeof(List<NetworkSegmentInfo>), 200)]
    public ActionResult<List<NetworkSegmentInfo>> GetSegments()
    {
        var segments = _options.NetworkSegments.Select(s => new NetworkSegmentInfo
        {
            Name = s.Name,
            NetworkCidr = s.NetworkCidr,
            Description = s.Description,
            EnableAutoDiscovery = s.EnableAutoDiscovery,
            DiscoveryIntervalMinutes = s.DiscoveryIntervalMinutes
        }).ToList();

        return Ok(segments);
    }

    private List<LocalNetworkSegment> GetConfiguredSegments()
    {
        return _options.NetworkSegments
            .Select(config => LocalNetworkSegment.Create(
                config.NetworkCidr,
                config.Name,
                config.EnableAutoDiscovery,
                config.DiscoveryIntervalMinutes))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value!)
            .ToList();
    }
}

// Request/Response DTOs

public class DiscoverSegmentRequest
{
    public string SegmentName { get; init; } = string.Empty;
}

public class DiscoveryResultResponse
{
    public int SegmentsScanned { get; init; }
    public int TotalDevicesFound { get; init; }
    public List<SegmentResult> SegmentResults { get; init; } = new();
}

public class SegmentResult
{
    public string SegmentName { get; init; } = string.Empty;
    public int DevicesFound { get; init; }
}

public class SegmentDiscoveryResponse
{
    public string SegmentName { get; init; } = string.Empty;
    public string NetworkCidr { get; init; } = string.Empty;
    public int DevicesFound { get; init; }
    public DateTime DiscoveredAt { get; init; }
}

public class DeviceIdentificationResponse
{
    public string IpAddress { get; init; } = string.Empty;
    public string? MacAddress { get; init; }
    public string? Hostname { get; init; }
    public string? Vendor { get; init; }
    public string? DeviceType { get; init; }
    public string? Model { get; init; }
    public bool IsReachable { get; init; }
}

public class NetworkSegmentInfo
{
    public string Name { get; init; } = string.Empty;
    public string NetworkCidr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool EnableAutoDiscovery { get; init; }
    public int DiscoveryIntervalMinutes { get; init; }
}
