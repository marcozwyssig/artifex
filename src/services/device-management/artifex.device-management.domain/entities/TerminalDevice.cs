using Artifex.DeviceManagement.Domain.ValueObjects;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Entities;

/// <summary>
/// Terminal Device entity - represents an unmanaged end device connected to a switch
/// Examples: PC, printer, IP phone, IoT device
/// </summary>
public class TerminalDevice : Entity<Guid>
{
    public Guid SwitchId { get; private set; }
    public Guid SwitchPortId { get; private set; }
    public MacAddress MacAddress { get; private set; }
    public IpAddress? IpAddress { get; private set; }
    public string? Hostname { get; private set; }
    public TerminalDeviceType Type { get; private set; }
    public string? Manufacturer { get; private set; }
    public string? Model { get; private set; }
    public string? Description { get; private set; }
    public bool IsOnline { get; private set; }
    public DateTime FirstSeenAt { get; private set; }
    public DateTime? LastSeenAt { get; private set; }
    public int Vlan { get; private set; }

    // EF Core requires a parameterless constructor
    private TerminalDevice() : base(Guid.Empty)
    {
        MacAddress = null!;
    }

    private TerminalDevice(
        Guid id,
        Guid switchId,
        Guid switchPortId,
        MacAddress macAddress,
        TerminalDeviceType type,
        int vlan) : base(id)
    {
        SwitchId = switchId;
        SwitchPortId = switchPortId;
        MacAddress = macAddress;
        Type = type;
        Vlan = vlan;
        IsOnline = true;
        FirstSeenAt = DateTime.UtcNow;
        LastSeenAt = DateTime.UtcNow;
    }

    public static Result<TerminalDevice> Create(
        Guid switchId,
        Guid switchPortId,
        MacAddress macAddress,
        TerminalDeviceType type = TerminalDeviceType.Unknown,
        int vlan = 1)
    {
        if (switchId == Guid.Empty)
        {
            return Result.Failure<TerminalDevice>("Switch ID cannot be empty");
        }

        if (switchPortId == Guid.Empty)
        {
            return Result.Failure<TerminalDevice>("Switch port ID cannot be empty");
        }

        if (vlan < 1 || vlan > 4094)
        {
            return Result.Failure<TerminalDevice>("VLAN must be between 1 and 4094");
        }

        var device = new TerminalDevice(
            Guid.NewGuid(),
            switchId,
            switchPortId,
            macAddress,
            type,
            vlan);

        return Result.Success(device);
    }

    public Result UpdateInformation(
        IpAddress? ipAddress = null,
        string? hostname = null,
        string? manufacturer = null,
        string? model = null,
        string? description = null)
    {
        if (ipAddress != null) IpAddress = ipAddress;
        if (hostname != null) Hostname = hostname;
        if (manufacturer != null) Manufacturer = manufacturer;
        if (model != null) Model = model;
        if (description != null) Description = description;

        return Result.Success();
    }

    public Result UpdateType(TerminalDeviceType newType)
    {
        Type = newType;
        return Result.Success();
    }

    public Result MarkAsOnline()
    {
        IsOnline = true;
        LastSeenAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkAsOffline()
    {
        IsOnline = false;
        return Result.Success();
    }

    public Result MoveToPort(Guid newSwitchId, Guid newSwitchPortId, int? newVlan = null)
    {
        if (newSwitchId == Guid.Empty)
        {
            return Result.Failure("New switch ID cannot be empty");
        }

        if (newSwitchPortId == Guid.Empty)
        {
            return Result.Failure("New switch port ID cannot be empty");
        }

        SwitchId = newSwitchId;
        SwitchPortId = newSwitchPortId;

        if (newVlan.HasValue)
        {
            if (newVlan.Value < 1 || newVlan.Value > 4094)
            {
                return Result.Failure("VLAN must be between 1 and 4094");
            }
            Vlan = newVlan.Value;
        }

        return Result.Success();
    }
}

public enum TerminalDeviceType
{
    Unknown = 0,
    Workstation = 1,
    Laptop = 2,
    Printer = 3,
    IPPhone = 4,
    AccessPoint = 5,
    IPCamera = 6,
    IoTDevice = 7,
    MobileDevice = 8,
    NetworkStorage = 9,
    VoIPGateway = 10
}
