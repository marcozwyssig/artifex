using Maestro.DeviceManagement.Api.Dtos;
using Maestro.Shared.Api.DTOs;
using Maestro.Shared.Api.Queries;

namespace Maestro.DeviceManagement.Api.Queries;

/// <summary>
/// Query to get all devices with pagination
/// </summary>
public class GetAllDevicesQuery : PagedQuery, IQuery<PagedResult<DeviceDto>>
{
    public string? Status { get; init; }
    public string? Type { get; init; }
    public string? Vendor { get; init; }
    public string? SearchTerm { get; init; }
}
