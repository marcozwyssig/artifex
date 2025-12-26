using Artifex.DeviceManagement.Cqrs.Dtos;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.DeviceManagement.Domain.Repositories;
using Artifex.Shared.Cqrs.DTOs;
using Artifex.Shared.Cqrs.Queries;

namespace Artifex.DeviceManagement.Cqrs.Queries;

/// <summary>
/// Handler for GetAllDevicesQuery
/// </summary>
public class GetAllDevicesQueryHandler : IQueryHandler<GetAllDevicesQuery, PagedResult<DeviceDto>>
{
    private readonly IDeviceRepository _deviceRepository;

    public GetAllDevicesQueryHandler(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<PagedResult<DeviceDto>> HandleAsync(GetAllDevicesQuery query, CancellationToken cancellationToken = default)
    {
        // Get all devices (filtering would be done at repository level in production)
        var allDevices = await _deviceRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filteredDevices = allDevices.AsEnumerable();

        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<DeviceStatus>(query.Status, true, out var status))
        {
            filteredDevices = filteredDevices.Where(d => d.Status == status);
        }

        if (!string.IsNullOrEmpty(query.Type) && Enum.TryParse<DeviceType>(query.Type, true, out var type))
        {
            filteredDevices = filteredDevices.Where(d => d.Type == type);
        }

        if (!string.IsNullOrEmpty(query.Vendor) && Enum.TryParse<Vendor>(query.Vendor, true, out var vendor))
        {
            filteredDevices = filteredDevices.Where(d => d.Vendor == vendor);
        }

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            filteredDevices = filteredDevices.Where(d =>
                d.Hostname.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                d.ManagementIp.Value.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (d.Location?.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var devicesList = filteredDevices.ToList();
        var totalCount = devicesList.Count;

        // Apply pagination
        var paginatedDevices = devicesList
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToList();

        // Map to DTOs
        var dtos = paginatedDevices.Select(device => new DeviceDto
        {
            Id = device.Id,
            Hostname = device.Hostname,
            ManagementIp = device.ManagementIp.Value,
            MacAddress = device.MacAddress?.Value,
            DeviceType = device.Type.ToString(),
            Status = device.Status.ToString(),
            Vendor = device.Vendor.ToString(),
            Model = device.Model,
            SerialNumber = device.SerialNumber,
            SoftwareVersion = device.SoftwareVersion,
            Location = device.Location,
            Description = device.Description,
            CreatedAt = device.CreatedAt,
            LastSeenAt = device.LastSeenAt,
            LastModifiedAt = device.LastModifiedAt,
            Interfaces = device.Interfaces.Select(i => new InterfaceDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                IpAddress = i.IpAddress?.Value,
                MacAddress = i.MacAddress?.Value,
                Type = i.Type.ToString(),
                Status = i.Status.ToString(),
                Speed = i.Speed,
                Mtu = i.Mtu,
                Vlan = i.Vlan,
                IsManagement = i.IsManagement
            }).ToList()
        }).ToList();

        return new PagedResult<DeviceDto>(dtos, totalCount, query.PageNumber, query.PageSize);
    }
}
