using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Entities;

/// <summary>
/// Represents a local network segment that should be scanned for devices
/// </summary>
public class LocalNetworkSegment : Entity<Guid>
{
     private LocalNetworkSegment(
        string networkCidr,
        string name,
        bool enableAutoDiscovery,
        int discoveryIntervalMinutes) : base(Guid.NewGuid())
    {
        NetworkCidr = networkCidr;
        Name = name;
        EnableAutoDiscovery = enableAutoDiscovery;
        DiscoveryIntervalMinutes = discoveryIntervalMinutes;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Network range in CIDR notation (e.g., "192.168.1.0/24")
    /// </summary>
    public string NetworkCidr { get; private set; } = string.Empty;

    /// <summary>
    /// Friendly name for this network segment
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of this network segment
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Whether automatic discovery is enabled for this segment
    /// </summary>
    public bool EnableAutoDiscovery { get; private set; }

    /// <summary>
    /// How often to run discovery (in minutes)
    /// </summary>
    public int DiscoveryIntervalMinutes { get; private set; }

    /// <summary>
    /// Last time discovery was run for this segment
    /// </summary>
    public DateTime? LastDiscoveryAt { get; private set; }

    /// <summary>
    /// Number of devices found in last discovery
    /// </summary>
    public int LastDeviceCount { get; private set; }

    /// <summary>
    /// When this segment was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new network segment
    /// </summary>
    public static Result<LocalNetworkSegment> Create(
        string networkCidr,
        string name,
        bool enableAutoDiscovery = true,
        int discoveryIntervalMinutes = 60)
    {
        // Validate CIDR notation
        if (string.IsNullOrWhiteSpace(networkCidr))
        {
            return Result.Failure<LocalNetworkSegment>("Network CIDR cannot be empty");
        }

        if (!IsValidCidr(networkCidr))
        {
            return Result.Failure<LocalNetworkSegment>($"Invalid CIDR notation: {networkCidr}");
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<LocalNetworkSegment>("Segment name cannot be empty");
        }

        // Validate interval
        if (discoveryIntervalMinutes < 1)
        {
            return Result.Failure<LocalNetworkSegment>("Discovery interval must be at least 1 minute");
        }

        var segment = new LocalNetworkSegment(
            networkCidr,
            name,
            enableAutoDiscovery,
            discoveryIntervalMinutes);

        return Result.Success(segment);
    }

    /// <summary>
    /// Determines if discovery should be run based on interval
    /// </summary>
    public bool ShouldRunDiscovery()
    {
        if (!EnableAutoDiscovery)
        {
            return false;
        }

        if (LastDiscoveryAt == null)
        {
            return true; // Never run before
        }

        var nextDiscoveryTime = LastDiscoveryAt.Value.AddMinutes(DiscoveryIntervalMinutes);
        return DateTime.UtcNow >= nextDiscoveryTime;
    }

    /// <summary>
    /// Mark that discovery was run
    /// </summary>
    public void MarkDiscoveryCompleted(int deviceCount)
    {
        LastDiscoveryAt = DateTime.UtcNow;
        LastDeviceCount = deviceCount;
    }

    /// <summary>
    /// Update discovery settings
    /// </summary>
    public Result UpdateSettings(bool enableAutoDiscovery, int discoveryIntervalMinutes)
    {
        if (discoveryIntervalMinutes < 1)
        {
            return Result.Failure("Discovery interval must be at least 1 minute");
        }

        EnableAutoDiscovery = enableAutoDiscovery;
        DiscoveryIntervalMinutes = discoveryIntervalMinutes;

        return Result.Success();
    }

    /// <summary>
    /// Set description
    /// </summary>
    public void SetDescription(string? description)
    {
        Description = description;
    }

    /// <summary>
    /// Validates CIDR notation format
    /// </summary>
    private static bool IsValidCidr(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        // Validate IP address
        if (!System.Net.IPAddress.TryParse(parts[0], out var ipAddress))
        {
            return false;
        }

        // Must be IPv4
        if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return false;
        }

        // Validate prefix length
        if (!int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        if (prefixLength < 0 || prefixLength > 32)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get time until next discovery
    /// </summary>
    public TimeSpan? GetTimeUntilNextDiscovery()
    {
        if (!EnableAutoDiscovery || LastDiscoveryAt == null)
        {
            return null;
        }

        var nextDiscoveryTime = LastDiscoveryAt.Value.AddMinutes(DiscoveryIntervalMinutes);
        var timeUntilNext = nextDiscoveryTime - DateTime.UtcNow;

        return timeUntilNext > TimeSpan.Zero ? timeUntilNext : TimeSpan.Zero;
    }

    public override string ToString()
    {
        return $"{Name} ({NetworkCidr}) - Auto: {EnableAutoDiscovery}, Interval: {DiscoveryIntervalMinutes}min";
    }
}
