namespace Maestro.DeviceManagement.Domain.Enums;

/// <summary>
/// Network segment where the device operates
/// </summary>
public enum NetworkSegment
{
    Unknown = 0,
    LAN = 1,          // Local Area Network
    WAN = 2,          // Wide Area Network
    DMZ = 3,          // Demilitarized Zone
    Management = 4,   // Management Network
    Storage = 5,      // Storage Network
    Backbone = 6      // Backbone Network
}
