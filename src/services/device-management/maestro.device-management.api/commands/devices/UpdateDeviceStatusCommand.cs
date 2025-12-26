using Maestro.Shared.Infrastructure.Commands;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Commands;

/// <summary>
/// Command to update device status
/// </summary>
public class UpdateDeviceStatusCommand : ICommand<Result>
{
    public Guid DeviceId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
