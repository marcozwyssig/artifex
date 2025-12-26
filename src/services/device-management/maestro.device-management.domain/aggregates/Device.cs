using Maestro.DeviceManagement.Domain.Enums;
using Maestro.DeviceManagement.Domain.Events;
using Maestro.DeviceManagement.Domain.Entities;
using Maestro.DeviceManagement.Domain.ValueObjects;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Aggregates;

/// <summary>
/// Device aggregate root - represents a network device in the system
/// </summary>
public class Device : AggregateRoot<Guid>
{
    public string Hostname { get; private set; }
    public IpAddress ManagementIp { get; private set; }
    public MacAddress? MacAddress { get; private set; }
    public DeviceType Type { get; private set; }
    public DeviceStatus Status { get; private set; }
    public Vendor Vendor { get; private set; }
    public string? Model { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? SoftwareVersion { get; private set; }
    public Credentials Credentials { get; private set; }
    public string? Location { get; private set; }
    public string? Description { get; private set; }
    public NetworkSegment NetworkSegment { get; private set; }
    public DeviceRole? Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastSeenAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    // Navigation properties for related entities
    private readonly List<Interface> _interfaces = new();
    public IReadOnlyCollection<Interface> Interfaces => _interfaces.AsReadOnly();

    // EF Core requires a parameterless constructor
    private Device() : base(Guid.Empty)
    {
        Hostname = string.Empty;
        ManagementIp = null!;
        Credentials = null!;
    }

    private Device(
        Guid id,
        string hostname,
        IpAddress managementIp,
        DeviceType type,
        Vendor vendor,
        Credentials credentials,
        NetworkSegment networkSegment) : base(id)
    {
        Hostname = hostname;
        ManagementIp = managementIp;
        Type = type;
        Vendor = vendor;
        Credentials = credentials;
        NetworkSegment = networkSegment;
        Status = DeviceStatus.Unknown;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new device
    /// </summary>
    public static Result<Device> Create(
        string hostname,
        IpAddress managementIp,
        DeviceType type,
        Vendor vendor,
        Credentials credentials,
        NetworkSegment networkSegment = NetworkSegment.Unknown)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            return Result.Failure<Device>("Hostname cannot be empty");
        }

        if (hostname.Length < 3)
        {
            return Result.Failure<Device>("Hostname must be at least 3 characters long");
        }

        var device = new Device(Guid.NewGuid(), hostname, managementIp, type, vendor, credentials, networkSegment);
        device.AddDomainEvent(new DeviceRegisteredEvent(
            device.Id,
            device.Hostname,
            device.ManagementIp.Value,
            device.Type.ToString()));

        return Result<Device>.Success(device);
    }

    /// <summary>
    /// Updates device information
    /// </summary>
    public Result UpdateInformation(
        string? hostname = null,
        string? model = null,
        string? serialNumber = null,
        string? softwareVersion = null,
        string? location = null,
        string? description = null)
    {
        if (hostname != null)
        {
            if (string.IsNullOrWhiteSpace(hostname))
            {
                return Result.Failure("Hostname cannot be empty");
            }
            Hostname = hostname;
        }

        if (model != null) Model = model;
        if (serialNumber != null) SerialNumber = serialNumber;
        if (softwareVersion != null) SoftwareVersion = softwareVersion;
        if (location != null) Location = location;
        if (description != null) Description = description;

        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Updates device status
    /// </summary>
    public Result ChangeStatus(DeviceStatus newStatus, string? reason = null)
    {
        if (Status == newStatus)
        {
            return Result.Success();
        }

        var oldStatus = Status;
        Status = newStatus;
        LastModifiedAt = DateTime.UtcNow;

        if (newStatus == DeviceStatus.Online)
        {
            LastSeenAt = DateTime.UtcNow;
        }

        AddDomainEvent(new DeviceStatusChangedEvent(Id, oldStatus, newStatus, reason));
        return Result.Success();
    }

    /// <summary>
    /// Sets device as online
    /// </summary>
    public Result SetOnline()
    {
        return ChangeStatus(DeviceStatus.Online, "Device became reachable");
    }

    /// <summary>
    /// Sets device as offline
    /// </summary>
    public Result SetOffline(string? reason = null)
    {
        return ChangeStatus(DeviceStatus.Offline, reason ?? "Device unreachable");
    }

    /// <summary>
    /// Updates device credentials
    /// </summary>
    public Result UpdateCredentials(Credentials newCredentials)
    {
        Credentials = newCredentials;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Sets the MAC address
    /// </summary>
    public Result SetMacAddress(MacAddress macAddress)
    {
        MacAddress = macAddress;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Adds an interface to the device
    /// </summary>
    public Result AddInterface(Interface @interface)
    {
        if (_interfaces.Any(i => i.Name == @interface.Name))
        {
            return Result.Failure($"Interface {@interface.Name} already exists on device");
        }

        _interfaces.Add(@interface);
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Sets the device role
    /// </summary>
    public Result SetRole(DeviceRole role)
    {
        var validationResult = role.Validate();
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Role = role;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Updates the network segment
    /// </summary>
    public Result UpdateNetworkSegment(NetworkSegment networkSegment)
    {
        NetworkSegment = networkSegment;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Gets required ports based on device role
    /// </summary>
    public IReadOnlyCollection<int> GetRequiredPorts()
    {
        if (Role != null)
        {
            return Role.GetRequiredPorts();
        }

        // Default ports if no role is set
        return new List<int> { 22, 161 }.AsReadOnly(); // SSH and SNMP
    }

    /// <summary>
    /// Marks device for deletion
    /// </summary>
    public void MarkAsDeleted()
    {
        AddDomainEvent(new DeviceDeletedEvent(Id, Hostname, ManagementIp.Value));
    }
}
