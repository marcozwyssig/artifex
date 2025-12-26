using Maestro.DeviceManagement.Api.Dtos;
using Maestro.DeviceManagement.Domain.Repositories;
using Maestro.Shared.Api.Queries;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Queries;

/// <summary>
/// Handler for GetDeviceByIdQuery
/// </summary>
public class GetDeviceByIdQueryHandler : IQueryHandler<GetDeviceByIdQuery, Result<DeviceDto>>
{
    private readonly IDeviceRepository _deviceRepository;

    public GetDeviceByIdQueryHandler(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<Result<DeviceDto>> HandleAsync(GetDeviceByIdQuery query, CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdAsync(query.DeviceId, cancellationToken);
        if (device == null)
        {
            return Result.Failure<DeviceDto>($"Device with ID {query.DeviceId} not found");
        }

        var dto = new DeviceDto
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
        };

        return Result<DeviceDto>.Success(dto);
    }
}
