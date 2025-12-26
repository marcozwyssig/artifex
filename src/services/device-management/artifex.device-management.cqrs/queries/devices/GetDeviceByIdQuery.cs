using Artifex.DeviceManagement.Cqrs.Dtos;
using Artifex.Shared.Cqrs.Queries;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Cqrs.Queries;

/// <summary>
/// Query to get a device by ID
/// </summary>
public class GetDeviceByIdQuery : IQuery<Result<DeviceDto>>
{
    public Guid DeviceId { get; init; }
}
