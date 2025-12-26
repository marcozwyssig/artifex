using Artifex.DeviceManagement.Domain.ValueObjects;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Aggregates;

/// <summary>
/// Network interface entity (part of Device aggregate)
/// </summary>
public class Interface : Entity<Guid>
{
    public Guid DeviceId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public IpAddress? IpAddress { get; private set; }
    public MacAddress? MacAddress { get; private set; }
    public InterfaceType Type { get; private set; }
    public InterfaceStatus Status { get; private set; }
    public long? Speed { get; private set; } // in bps
    public int? Mtu { get; private set; }
    public int? Vlan { get; private set; }
    public bool IsManagement { get; private set; }
    public DateTime? LastStatusChange { get; private set; }

    // EF Core requires a parameterless constructor
    private Interface() : base(Guid.Empty)
    {
        Name = string.Empty;
    }

    private Interface(
        Guid id,
        Guid deviceId,
        string name,
        InterfaceType type) : base(id)
    {
        DeviceId = deviceId;
        Name = name;
        Type = type;
        Status = InterfaceStatus.Unknown;
    }

    public static Result<Interface> Create(Guid deviceId, string name, InterfaceType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Interface>("Interface name cannot be empty");
        }

        var @interface = new Interface(Guid.NewGuid(), deviceId, name, type);
        return Result.Success(@interface);
    }

    public Result UpdateStatus(InterfaceStatus newStatus)
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
        int? vlan = null)
    {
        if (description != null) Description = description;
        if (ipAddress != null) IpAddress = ipAddress;
        if (macAddress != null) MacAddress = macAddress;
        if (speed.HasValue) Speed = speed.Value;
        if (mtu.HasValue) Mtu = mtu.Value;
        if (vlan.HasValue) Vlan = vlan.Value;

        return Result.Success();
    }

    public void SetAsManagement()
    {
        IsManagement = true;
    }
}

public enum InterfaceType
{
    Unknown = 0,
    Ethernet = 1,
    FastEthernet = 2,
    GigabitEthernet = 3,
    TenGigabitEthernet = 4,
    Loopback = 5,
    Vlan = 6,
    PortChannel = 7,
    Tunnel = 8
}

public enum InterfaceStatus
{
    Unknown = 0,
    Up = 1,
    Down = 2,
    AdminDown = 3,
    Testing = 4
}
