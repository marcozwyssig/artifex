using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Artifex.DeviceManagement.Api.Services;

/// <summary>
/// HTTP client for communicating with the Device Management service
/// </summary>
public class DeviceManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeviceManagementClient> _logger;

    public DeviceManagementClient(HttpClient httpClient, ILogger<DeviceManagementClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Triggers device discovery on the Device Management service
    /// </summary>
    public async Task<DeviceDiscoveryResponse?> DiscoverDevicesAsync(
        string networkCidr,
        bool autoRegister = true,
        string? defaultUsername = null,
        string? defaultPassword = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DiscoverDevicesRequest
            {
                NetworkCidr = networkCidr,
                AutoRegister = autoRegister,
                DefaultUsername = defaultUsername,
                DefaultPassword = defaultPassword
            };

            _logger.LogInformation("Sending discovery request for network {NetworkCidr}", networkCidr);

            var response = await _httpClient.PostAsJsonAsync(
                "/api/discovery/discover",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Discovery request failed with status {StatusCode}: {Reason}",
                    response.StatusCode,
                    response.ReasonPhrase);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<DeviceDiscoveryResponse>(
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Discovery completed: Found {Found}, Registered {Registered}",
                result?.DevicesFound ?? 0,
                result?.DevicesRegistered ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during device discovery request");
            return null;
        }
    }

    /// <summary>
    /// Identifies a single device
    /// </summary>
    public async Task<DiscoveredDeviceInfo?> IdentifyDeviceAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Identifying device at {IpAddress}", ipAddress);

            var response = await _httpClient.GetAsync(
                $"/api/discovery/identify/{ipAddress}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DiscoveredDeviceInfo>(
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying device {IpAddress}", ipAddress);
            return null;
        }
    }

    /// <summary>
    /// Reports device status to the Device Management service
    /// </summary>
    public async Task<bool> ReportDeviceStatusAsync(
        Guid deviceId,
        string status,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                Status = status,
                Reason = reason
            };

            var response = await _httpClient.PatchAsJsonAsync(
                $"/api/devices/{deviceId}/status",
                request,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting device status for {DeviceId}", deviceId);
            return false;
        }
    }
}

// DTOs for communication with Device Management service

public class DiscoverDevicesRequest
{
    public string NetworkCidr { get; init; } = string.Empty;
    public bool AutoRegister { get; init; }
    public string? DefaultUsername { get; init; }
    public string? DefaultPassword { get; init; }
}

public class DeviceDiscoveryResponse
{
    public int TotalScanned { get; init; }
    public int DevicesFound { get; init; }
    public int DevicesRegistered { get; init; }
    public DateTime DiscoveryStartedAt { get; init; }
    public DateTime DiscoveryCompletedAt { get; init; }
    public List<DiscoveredDeviceInfo> Devices { get; init; } = new();
}

public class DiscoveredDeviceInfo
{
    public string IpAddress { get; init; } = string.Empty;
    public string? MacAddress { get; init; }
    public string? Hostname { get; init; }
    public string? Vendor { get; init; }
    public string? DeviceType { get; init; }
    public string? Model { get; init; }
    public bool IsReachable { get; init; }
}
