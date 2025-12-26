using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Api.Commands;

/// <summary>
/// Command to delete a device
/// </summary>
public class DeleteDeviceCommand : ICommand<Result>
{
    public Guid DeviceId { get; init; }
}
