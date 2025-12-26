using Maestro.Shared.Infrastructure.Commands;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Commands;

/// <summary>
/// Command to delete a device
/// </summary>
public class DeleteDeviceCommand : ICommand<Result>
{
    public Guid DeviceId { get; init; }
}
