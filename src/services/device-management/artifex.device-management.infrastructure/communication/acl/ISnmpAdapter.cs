namespace Artifex.DeviceManagement.Infrastructure.Communication.Acl;

/// <summary>
/// Anti-Corruption Layer interface for SNMP communication
/// Translates between external SNMP library and domain model
/// </summary>
public interface ISnmpAdapter
{
    /// <summary>
    /// Queries device information via SNMP
    /// </summary>
    Task<SnmpDeviceInfo?> QueryDeviceAsync(
        string ipAddress,
        string community = "public",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if device is reachable via SNMP
    /// </summary>
    Task<bool> IsReachableAsync(
        string ipAddress,
        string community = "public",
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Domain model for SNMP device information
/// Isolated from external SNMP library types
/// </summary>
public record SnmpDeviceInfo(
    string SystemDescription,
    string SystemName,
    string SystemObjectId,
    string? SystemLocation,
    string? SystemContact,
    TimeSpan? Uptime,
    Dictionary<string, string> AdditionalProperties);
