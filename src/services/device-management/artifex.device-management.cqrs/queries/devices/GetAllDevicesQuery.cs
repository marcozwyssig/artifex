using Artifex.DeviceManagement.Cqrs.Dtos;
using Artifex.Shared.Cqrs.DTOs;
using Artifex.Shared.Cqrs.Queries;

namespace Artifex.DeviceManagement.Cqrs.Queries;

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
