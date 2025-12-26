using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Cqrs.Commands;

/// <summary>
/// Command to update device status
/// </summary>
public class UpdateDeviceStatusCommand : ICommand<Result>
{
    public Guid DeviceId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
