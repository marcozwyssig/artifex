namespace Maestro.DeviceManagement.Api.Dtos;

/// <summary>
/// DTO for a discovered device (from network scanning)
/// </summary>
public class DiscoveredDeviceDto
{
    /// <summary>
    /// IP address of the device
    /// </summary>
    public string IpAddress { get; init; } = string.Empty;

    /// <summary>
    /// MAC address of the device
    /// </summary>
    public string? MacAddress { get; init; }

    /// <summary>
    /// Hostname from DNS or SNMP
    /// </summary>
    public string? Hostname { get; init; }

    /// <summary>
    /// Device vendor (from SNMP sysDescr)
    /// </summary>
    public string? Vendor { get; init; }

    /// <summary>
    /// Device type (Router, Switch, Server, etc.)
    /// </summary>
    public string? DeviceType { get; init; }

    /// <summary>
    /// Device model
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// SNMP system description
    /// </summary>
    public string? SystemDescription { get; init; }

    /// <summary>
    /// Whether the device responded to ping
    /// </summary>
    public bool IsReachable { get; init; }

    /// <summary>
    /// Whether SNMP is enabled and accessible
    /// </summary>
    public bool SnmpEnabled { get; init; }

    /// <summary>
    /// SNMP community string that worked (if any)
    /// </summary>
    public string? SnmpCommunity { get; init; }

    /// <summary>
    /// When the device was discovered
    /// </summary>
    public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public int? ResponseTimeMs { get; init; }
}
