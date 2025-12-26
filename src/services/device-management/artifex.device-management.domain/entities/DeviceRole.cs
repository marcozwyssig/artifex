using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Entities;

/// <summary>
/// Base entity for device roles - represents the function/purpose of a device
/// Must be navigatable from Device
/// </summary>
public abstract class DeviceRole : Entity<Guid>
{
    public Guid DeviceId { get; protected set; }
    public string RoleName { get; protected set; }
    public string? Description { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? LastModifiedAt { get; protected set; }

    protected DeviceRole() : base(Guid.Empty)
    {
        RoleName = string.Empty;
    }

    protected DeviceRole(Guid id, Guid deviceId, string roleName, string? description = null) : base(id)
    {
        DeviceId = deviceId;
        RoleName = roleName;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates if the role configuration is valid
    /// </summary>
    public abstract Result Validate();

    /// <summary>
    /// Gets the required ports for this role
    /// </summary>
    public abstract IReadOnlyCollection<int> GetRequiredPorts();

    protected void UpdateModifiedTimestamp()
    {
        LastModifiedAt = DateTime.UtcNow;
    }
}
