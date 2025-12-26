using Maestro.DeviceManagement.Api.Dtos;
using Maestro.Shared.Api.Queries;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Queries;

/// <summary>
/// Query to get a device by ID
/// </summary>
public class GetDeviceByIdQuery : IQuery<Result<DeviceDto>>
{
    public Guid DeviceId { get; init; }
}
