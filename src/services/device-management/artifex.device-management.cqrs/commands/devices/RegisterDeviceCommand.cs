using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Cqrs.Commands;

/// <summary>
/// Command to register a new device
/// </summary>
public class RegisterDeviceCommand : ICommand<Result<Guid>>
{
    public string Hostname { get; init; } = string.Empty;
    public string ManagementIp { get; init; } = string.Empty;
    public string DeviceType { get; init; } = string.Empty;
    public string Vendor { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? EnablePassword { get; init; }
    public string? Location { get; init; }
    public string? Description { get; init; }
}
