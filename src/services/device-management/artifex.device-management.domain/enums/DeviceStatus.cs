namespace Artifex.DeviceManagement.Domain.Enums;

/// <summary>
/// Status of a network device
/// </summary>
public enum DeviceStatus
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Maintenance = 3,
    Failed = 4,
    Discovering = 5
}
