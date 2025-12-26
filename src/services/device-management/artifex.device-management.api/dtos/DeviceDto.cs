namespace Artifex.DeviceManagement.Api.Dtos;

/// <summary>
/// Device data transfer object
/// </summary>
public class DeviceDto
{
    public Guid Id { get; init; }
    public string Hostname { get; init; } = string.Empty;
    public string ManagementIp { get; init; } = string.Empty;
    public string? MacAddress { get; init; }
    public string DeviceType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Vendor { get; init; } = string.Empty;
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? SoftwareVersion { get; init; }
    public string? Location { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastSeenAt { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public List<InterfaceDto> Interfaces { get; init; } = new();
}

/// <summary>
/// Interface data transfer object
/// </summary>
public class InterfaceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? IpAddress { get; init; }
    public string? MacAddress { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long? Speed { get; init; }
    public int? Mtu { get; init; }
    public int? Vlan { get; init; }
    public bool IsManagement { get; init; }
}
