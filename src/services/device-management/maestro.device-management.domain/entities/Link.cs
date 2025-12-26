using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Entities;

/// <summary>
/// Link entity - represents a physical/logical connection between two devices through their ports
/// </summary>
public class Link : Entity<Guid>
{
    public Guid SourceDeviceId { get; private set; }
    public Guid SourcePortId { get; private set; }
    public Guid DestinationDeviceId { get; private set; }
    public Guid DestinationPortId { get; private set; }
    public LinkType Type { get; private set; }
    public LinkStatus Status { get; private set; }
    public string? Description { get; private set; }
    public long? BandwidthInBps { get; private set; }
    public int? LatencyInMs { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastSeenAt { get; private set; }

    // Navigation properties (not persisted)
    private Port? _sourcePort;
    private Port? _destinationPort;

    // EF Core requires a parameterless constructor
    private Link() : base(Guid.Empty)
    {
    }

    private Link(
        Guid id,
        Guid sourceDeviceId,
        Guid sourcePortId,
        Guid destinationDeviceId,
        Guid destinationPortId,
        LinkType type) : base(id)
    {
        SourceDeviceId = sourceDeviceId;
        SourcePortId = sourcePortId;
        DestinationDeviceId = destinationDeviceId;
        DestinationPortId = destinationPortId;
        Type = type;
        Status = LinkStatus.Unknown;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Link> Create(
        Guid sourceDeviceId,
        Guid sourcePortId,
        Guid destinationDeviceId,
        Guid destinationPortId,
        LinkType type = LinkType.Ethernet)
    {
        if (sourceDeviceId == destinationDeviceId)
        {
            return Result.Failure<Link>("Cannot create link from device to itself");
        }

        if (sourcePortId == Guid.Empty || destinationPortId == Guid.Empty)
        {
            return Result.Failure<Link>("Source and destination ports must be specified");
        }

        var link = new Link(
            Guid.NewGuid(),
            sourceDeviceId,
            sourcePortId,
            destinationDeviceId,
            destinationPortId,
            type);

        return Result.Success(link);
    }

    public Result UpdateStatus(LinkStatus newStatus)
    {
        Status = newStatus;

        if (newStatus == LinkStatus.Up)
        {
            LastSeenAt = DateTime.UtcNow;
        }

        return Result.Success();
    }

    public Result UpdateMetrics(long? bandwidthInBps = null, int? latencyInMs = null)
    {
        if (bandwidthInBps.HasValue) BandwidthInBps = bandwidthInBps.Value;
        if (latencyInMs.HasValue) LatencyInMs = latencyInMs.Value;
        return Result.Success();
    }

    public Result SetDescription(string description)
    {
        Description = description;
        return Result.Success();
    }

    /// <summary>
    /// Checks if the link is bidirectional (connects the same devices in both directions)
    /// </summary>
    public bool IsBidirectionalWith(Link otherLink)
    {
        return SourceDeviceId == otherLink.DestinationDeviceId &&
               DestinationDeviceId == otherLink.SourceDeviceId;
    }
}

public enum LinkType
{
    Unknown = 0,
    Ethernet = 1,
    Fiber = 2,
    Wireless = 3,
    Tunnel = 4,
    Virtual = 5,
    MPLS = 6
}

public enum LinkStatus
{
    Unknown = 0,
    Up = 1,
    Down = 2,
    Degraded = 3,
    Testing = 4
}
