using Artifex.DeviceManagement.Domain.ValueObjects;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Entities;

/// <summary>
/// Network port entity (part of Device aggregate)
/// Renamed from Interface to be more accurate
/// </summary>
public class Port : Entity<Guid>
{
    public Guid DeviceId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public IpAddress? IpAddress { get; private set; }
    public MacAddress? MacAddress { get; private set; }
    public PortType Type { get; private set; }
    public PortStatus Status { get; private set; }
    public long? Speed { get; private set; } // in bps
    public int? Mtu { get; private set; }
    public int? Vlan { get; private set; }
    public bool IsManagement { get; private set; }
    public bool IsTrunk { get; private set; }
    public DateTime? LastStatusChange { get; private set; }

    // EF Core requires a parameterless constructor
    private Port() : base(Guid.Empty)
    {
        Name = string.Empty;
    }

    private Port(
        Guid id,
        Guid deviceId,
        string name,
        PortType type) : base(id)
    {
        DeviceId = deviceId;
        Name = name;
        Type = type;
        Status = PortStatus.Unknown;
    }

    public static Result<Port> Create(Guid deviceId, string name, PortType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Port>("Port name cannot be empty");
        }

        var port = new Port(Guid.NewGuid(), deviceId, name, type);
        return Result.Success(port);
    }

    public Result UpdateStatus(PortStatus newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
            LastStatusChange = DateTime.UtcNow;
        }
        return Result.Success();
    }

    public Result UpdateConfiguration(
        string? description = null,
        IpAddress? ipAddress = null,
        MacAddress? macAddress = null,
        long? speed = null,
        int? mtu = null,
        int? vlan = null,
        bool? isTrunk = null)
    {
        if (description != null) Description = description;
        if (ipAddress != null) IpAddress = ipAddress;
        if (macAddress != null) MacAddress = macAddress;
        if (speed.HasValue) Speed = speed.Value;
        if (mtu.HasValue) Mtu = mtu.Value;
        if (vlan.HasValue) Vlan = vlan.Value;
        if (isTrunk.HasValue) IsTrunk = isTrunk.Value;

        return Result.Success();
    }

    public void SetAsManagement()
    {
        IsManagement = true;
    }
}

public enum PortType
{
    Unknown = 0,
    Ethernet = 1,
    FastEthernet = 2,
    GigabitEthernet = 3,
    TenGigabitEthernet = 4,
    TwentyFiveGigabitEthernet = 5,
    FortyGigabitEthernet = 6,
    HundredGigabitEthernet = 7,
    Loopback = 8,
    Vlan = 9,
    PortChannel = 10,
    Tunnel = 11
}

public enum PortStatus
{
    Unknown = 0,
    Up = 1,
    Down = 2,
    AdminDown = 3,
    Testing = 4,
    Dormant = 5
}
