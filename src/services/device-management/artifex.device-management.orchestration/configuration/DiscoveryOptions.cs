namespace Artifex.DeviceManagement.Orchestration.Configuration;

/// <summary>
/// Configuration options for network discovery
/// </summary>
public class DiscoveryOptions
{
    public const string SectionName = "Discovery";

    /// <summary>
    /// Initial delay before starting discovery (in seconds)
    /// </summary>
    public int InitialDelaySeconds { get; set; } = 30;

    /// <summary>
    /// How often to check if discovery should run (in minutes)
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Default SNMP community string for discovery
    /// </summary>
    public string DefaultCommunityString { get; set; } = "public";

    /// <summary>
    /// Default username for device authentication
    /// </summary>
    public string? DefaultUsername { get; set; }

    /// <summary>
    /// Default password for device authentication
    /// </summary>
    public string? DefaultPassword { get; set; }

    /// <summary>
    /// Timeout for ping operations (in milliseconds)
    /// </summary>
    public int PingTimeoutMs { get; set; } = 1000;

    /// <summary>
    /// Timeout for SNMP operations (in milliseconds)
    /// </summary>
    public int SnmpTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Maximum number of concurrent discovery operations
    /// </summary>
    public int MaxConcurrentDiscoveries { get; set; } = 50;

    /// <summary>
    /// Network segments to discover
    /// </summary>
    public List<NetworkSegmentConfiguration> NetworkSegments { get; set; } = new();
}

/// <summary>
/// Configuration for a network segment
/// </summary>
public class NetworkSegmentConfiguration
{
    /// <summary>
    /// Unique name for this segment
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Network range in CIDR notation (e.g., "192.168.1.0/24")
    /// </summary>
    public string NetworkCidr { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Enable automatic discovery for this segment
    /// </summary>
    public bool EnableAutoDiscovery { get; set; } = true;

    /// <summary>
    /// How often to run discovery (in minutes)
    /// </summary>
    public int DiscoveryIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// SNMP community string for this segment (overrides default)
    /// </summary>
    public string? CommunityString { get; set; }

    /// <summary>
    /// Override default credentials for this segment
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Override default credentials for this segment
    /// </summary>
    public string? Password { get; set; }
}
