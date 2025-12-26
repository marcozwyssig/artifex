using Artifex.DeviceManagement.Api.Dtos;
using Artifex.Shared.Api.DTOs;
using Artifex.Shared.Api.Queries;

namespace Artifex.DeviceManagement.Api.Queries;

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
