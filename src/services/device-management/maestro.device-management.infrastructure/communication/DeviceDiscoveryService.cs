using Maestro.DeviceManagement.Domain.Services;
using Maestro.DeviceManagement.Infrastructure.Communication.Snmp;
using Maestro.Shared.Domain;
using Microsoft.Extensions.Logging;

namespace Maestro.DeviceManagement.Infrastructure.Communication;

/// <summary>
/// Implementation of device discovery service
/// </summary>
public class DeviceDiscoveryService : IDeviceDiscoveryService
{
    private readonly NetworkScanner _networkScanner;
    private readonly SnmpDeviceIdentifier _snmpIdentifier;
    private readonly ILogger<DeviceDiscoveryService> _logger;

    public DeviceDiscoveryService(
        NetworkScanner networkScanner,
        SnmpDeviceIdentifier snmpIdentifier,
        ILogger<DeviceDiscoveryService> logger)
    {
        _networkScanner = networkScanner;
        _snmpIdentifier = snmpIdentifier;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyCollection<DiscoveredDevice>>> DiscoverDevicesAsync(
        string networkCidr,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting device discovery for network {NetworkCidr}", networkCidr);

            // Step 1: Scan network for active IPs
            var activeIps = await _networkScanner.ScanNetworkRangeAsync(networkCidr, cancellationToken: cancellationToken);

            _logger.LogInformation("Found {Count} active IP addresses", activeIps.Count);

            // Step 2: Identify each device
            var identificationTasks = activeIps.Select(ip => IdentifyDeviceAsync(ip, cancellationToken));
            var results = await Task.WhenAll(identificationTasks);

            // Step 3: Filter out failed identifications
            var discoveredDevices = results
                .Where(r => r.IsSuccess)
                .Select(r => r.Value!)
                .ToList();

            _logger.LogInformation("Successfully identified {Count} devices", discoveredDevices.Count);

            return Result.Success<IReadOnlyCollection<DiscoveredDevice>>(discoveredDevices.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Device discovery failed for network {NetworkCidr}", networkCidr);
            return Result.Failure<IReadOnlyCollection<DiscoveredDevice>>($"Discovery failed: {ex.Message}");
        }
    }

    public async Task<Result<DiscoveredDevice>> IdentifyDeviceAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Identifying device at {IpAddress}", ipAddress);

            var discoveredDevice = new DiscoveredDevice
            {
                IpAddress = ipAddress,
                DiscoveredAt = DateTime.UtcNow
            };

            // Step 1: Check if device is reachable
            var isReachable = await _networkScanner.PingHostAsync(ipAddress, cancellationToken: cancellationToken);
            discoveredDevice.IsReachable = isReachable;

            if (!isReachable)
            {
                _logger.LogDebug("Device {IpAddress} is not reachable", ipAddress);
            }

            // Step 2: Try to resolve hostname
            var hostname = await _networkScanner.ResolveHostnameAsync(ipAddress, cancellationToken);
            if (!string.IsNullOrEmpty(hostname))
            {
                discoveredDevice.Hostname = hostname;
            }

            // Step 3: Try to get MAC address (local subnet only)
            var macAddress = _networkScanner.GetMacAddressFromArp(ipAddress);
            if (!string.IsNullOrEmpty(macAddress))
            {
                discoveredDevice.MacAddress = macAddress;
            }

            // Step 4: Try SNMP identification
            var snmpInfo = await _snmpIdentifier.QueryDeviceInfoAsync(ipAddress, cancellationToken: cancellationToken);

            if (snmpInfo.TryGetValue("sysDescr", out var sysDescr))
            {
                var (vendor, deviceType, model) = _snmpIdentifier.ParseSystemDescription(sysDescr);
  

                discoveredDevice.Vendor = vendor;
                discoveredDevice.DeviceType = deviceType;
                discoveredDevice.Model = model;

                foreach (var kvp in snmpInfo)
                {
                    discoveredDevice.AdditionalInfo[kvp.Key] = kvp.Value;
                }
            }

            // Step 5: Try SSH/API identification (future enhancement)
            // Could probe for common management ports: 22 (SSH), 443 (HTTPS API)

            _logger.LogInformation(
                "Device identified: IP={IpAddress}, Hostname={Hostname}, Vendor={Vendor}, Type={Type}",
                discoveredDevice.IpAddress, discoveredDevice.Hostname, discoveredDevice.Vendor, discoveredDevice.DeviceType);

            return Result.Success(discoveredDevice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to identify device at {IpAddress}", ipAddress);
            return Result.Failure<DiscoveredDevice>($"Identification failed: {ex.Message}");
        }
    }

    public async Task<bool> IsDeviceReachableAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        return await _networkScanner.PingHostAsync(ipAddress, cancellationToken: cancellationToken);
    }
}
