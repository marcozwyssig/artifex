using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;

namespace Artifex.DeviceManagement.Infrastructure.Communication;

/// <summary>
/// Network scanner for discovering active devices
/// </summary>
public class NetworkScanner
{
    private readonly ILogger<NetworkScanner> _logger;

    public NetworkScanner(ILogger<NetworkScanner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Scans a network range and returns active IP addresses
    /// </summary>
    public async Task<IReadOnlyCollection<string>> ScanNetworkRangeAsync(
        string networkCidr,
        int timeoutMs = 1000,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting network scan for {NetworkCidr}", networkCidr);

        var ipAddresses = ParseCidr(networkCidr);
        var activeIps = new List<string>();

        // Scan in parallel for better performance
        var tasks = ipAddresses.Select(ip => PingHostAsync(ip, timeoutMs, cancellationToken));
        var results = await Task.WhenAll(tasks);

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i])
            {
                activeIps.Add(ipAddresses[i]);
            }
        }

        _logger.LogInformation("Network scan complete. Found {Count} active devices", activeIps.Count);

        return activeIps.AsReadOnly();
    }

    /// <summary>
    /// Pings a host to check if it's reachable
    /// </summary>
    public async Task<bool> PingHostAsync(
        string ipAddress,
        int timeoutMs = 1000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, timeoutMs);
            return reply.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ping failed for {IpAddress}", ipAddress);
            return false;
        }
    }

    /// <summary>
    /// Resolves hostname for an IP address
    /// </summary>
    public async Task<string?> ResolveHostnameAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(ipAddress);
            return hostEntry.HostName;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Hostname resolution failed for {IpAddress}", ipAddress);
            return null;
        }
    }

    /// <summary>
    /// Gets MAC address for an IP (works only on local subnet via ARP)
    /// </summary>
    public string? GetMacAddressFromArp(string ipAddress)
    {
        try
        {
            // Note: This is platform-specific and would need different implementations
            // for Windows, Linux, and macOS
            // Placeholder implementation
            _logger.LogDebug("ARP lookup for {IpAddress}", ipAddress);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ARP lookup failed for {IpAddress}", ipAddress);
            return null;
        }
    }

    /// <summary>
    /// Parses CIDR notation to list of IP addresses
    /// </summary>
    private List<string> ParseCidr(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid CIDR notation: {cidr}");
        }

        var baseIp = IPAddress.Parse(parts[0]);
        var prefixLength = int.Parse(parts[1]);

        if (prefixLength < 0 || prefixLength > 32)
        {
            throw new ArgumentException($"Invalid prefix length: {prefixLength}");
        }

        var ipAddresses = new List<string>();
        var baseBytes = baseIp.GetAddressBytes();
        var hostBits = 32 - prefixLength;
        var numHosts = (int)Math.Pow(2, hostBits);

        // Limit scan to reasonable sizes (prevent scanning entire internet)
        if (numHosts > 1024)
        {
            _logger.LogWarning("Network range too large ({NumHosts} hosts). Limiting to first 1024.", numHosts);
            numHosts = 1024;
        }

        var baseIpInt = BitConverter.ToUInt32(baseBytes.Reverse().ToArray(), 0);

        for (uint i = 1; i < numHosts - 1; i++) // Skip network and broadcast addresses
        {
            var currentIpInt = baseIpInt + i;
            var currentIpBytes = BitConverter.GetBytes(currentIpInt).Reverse().ToArray();
            var currentIp = new IPAddress(currentIpBytes);
            ipAddresses.Add(currentIp.ToString());
        }

        return ipAddresses;
    }
}
