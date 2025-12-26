namespace Artifex.DeviceManagement.Domain.Enums;

/// <summary>
/// Type of network device
/// </summary>
public enum DeviceType
{
    Unknown = 0,
    Router = 1,
    Switch = 2,
    Firewall = 3,
    AccessPoint = 4,
    Controller = 5,
    LoadBalancer = 6,
    Server = 7
}
