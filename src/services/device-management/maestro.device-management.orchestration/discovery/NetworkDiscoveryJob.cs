using Maestro.DeviceManagement.Orchestration.Configuration;
using Maestro.DeviceManagement.Orchestration.Services.Discovery;
using Maestro.DeviceManagement.Orchestration.Services.Registration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maestro.DeviceManagement.Orchestration.Discovery;

/// <summary>
/// Background job that runs periodic network discovery
/// Scans configured network segments and auto-registers discovered devices
/// </summary>
public class NetworkDiscoveryJob : IHostedService, IDisposable
{
    private readonly INetworkDiscoveryService _discoveryService;
    private readonly IDeviceRegistrationService _registrationService;
    private readonly IOptions<DiscoveryOptions> _options;
    private readonly ILogger<NetworkDiscoveryJob> _logger;
    private Timer? _timer;
    private readonly Dictionary<string, DateTime> _lastDiscoveryTimes = new();

    public NetworkDiscoveryJob(
        INetworkDiscoveryService discoveryService,
        IDeviceRegistrationService registrationService,
        IOptions<DiscoveryOptions> options,
        ILogger<NetworkDiscoveryJob> logger)
    {
        _discoveryService = discoveryService;
        _registrationService = registrationService;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Network Discovery Job starting...");
        _logger.LogInformation("Initial delay: {Delay} seconds", _options.Value.InitialDelaySeconds);
        _logger.LogInformation("Check interval: {Interval} minutes", _options.Value.CheckIntervalMinutes);
        _logger.LogInformation("Configured segments: {Count}", _options.Value.NetworkSegments.Count);

        // Wait for initial delay before starting
        await Task.Delay(
            TimeSpan.FromSeconds(_options.Value.InitialDelaySeconds),
            cancellationToken);

        // Start periodic timer
        _timer = new Timer(
            ExecuteDiscovery,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(_options.Value.CheckIntervalMinutes));

        _logger.LogInformation("Network Discovery Job started successfully");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Network Discovery Job stopping...");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void ExecuteDiscovery(object? state)
    {
        try
        {
            _logger.LogInformation("Starting autonomous network discovery cycle");

            var discoveryTasks = new List<Task>();

            foreach (var segment in _options.Value.NetworkSegments)
            {
                if (!ShouldDiscoverSegment(segment))
                {
                    _logger.LogDebug(
                        "Skipping segment {Segment} - not ready for discovery",
                        segment.Name);
                    continue;
                }

                // Run discovery for this segment
                discoveryTasks.Add(DiscoverSegmentAsync(segment));
            }

            // Wait for all segments to complete
            await Task.WhenAll(discoveryTasks);

            _logger.LogInformation("Network discovery cycle completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during network discovery cycle");
        }
    }

    private async Task DiscoverSegmentAsync(NetworkSegmentConfiguration segment)
    {
        try
        {
            _logger.LogInformation(
                "Discovering segment: {Name} ({Cidr})",
                segment.Name,
                segment.NetworkCidr);

            // Discover devices in this segment
            var result = await _discoveryService.DiscoverNetworkAsync(
                segment.NetworkCidr,
                CancellationToken.None);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to discover segment {Segment}: {Error}",
                    segment.Name,
                    result.Error);
                return;
            }

            var discoveredDevices = result.Value!;
            _logger.LogInformation(
                "Discovered {Count} devices in segment {Segment}",
                discoveredDevices.Count,
                segment.Name);

            // Auto-register each discovered device
            var registrationTasks = discoveredDevices
                .Select(device => _registrationService.AutoRegisterDeviceAsync(
                    device,
                    CancellationToken.None))
                .ToList();

            var registrationResults = await Task.WhenAll(registrationTasks);

            var successCount = registrationResults.Count(r => r.IsSuccess);
            var failureCount = registrationResults.Count(r => r.IsFailure);

            _logger.LogInformation(
                "Auto-registration completed for segment {Segment}: {Success} successful, {Failures} failed",
                segment.Name,
                successCount,
                failureCount);

            // Update last discovery time
            _lastDiscoveryTimes[segment.Name] = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error discovering segment {Segment}",
                segment.Name);
        }
    }

    private bool ShouldDiscoverSegment(NetworkSegmentConfiguration segment)
    {
        // Check if auto-discovery is enabled
        if (!segment.EnableAutoDiscovery)
        {
            return false;
        }

        // Check if enough time has passed since last discovery
        if (_lastDiscoveryTimes.TryGetValue(segment.Name, out var lastDiscovery))
        {
            var nextDiscoveryTime = lastDiscovery.AddMinutes(segment.DiscoveryIntervalMinutes);
            if (DateTime.UtcNow < nextDiscoveryTime)
            {
                return false;
            }
        }

        return true;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
