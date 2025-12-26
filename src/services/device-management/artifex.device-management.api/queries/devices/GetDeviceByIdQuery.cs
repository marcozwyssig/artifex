using Artifex.DeviceManagement.Api.Dtos;
using Artifex.Shared.Api.Queries;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Api.Queries;

/// <summary>
/// Query to get a device by ID
/// </summary>
public class GetDeviceByIdQuery : IQuery<Result<DeviceDto>>
{
    public Guid DeviceId { get; init; }
}
