using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.Enums;
using Maestro.DeviceManagement.Domain.Repositories;
using Maestro.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maestro.DeviceManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Device repository implementation using Entity Framework Core
/// </summary>
public class DeviceRepository : BaseRepository<Device, Guid>, IDeviceRepository
{
    private readonly DeviceManagementDbContext _context;

    public DeviceRepository(DeviceManagementDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Interfaces)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyCollection<Device>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Interfaces)
            .ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByHostnameAsync(string hostname, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Interfaces)
            .FirstOrDefaultAsync(d => d.Hostname == hostname, cancellationToken);
    }

    public async Task<Device?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Interfaces)
            .FirstOrDefaultAsync(d => d.ManagementIp.Value == ipAddress, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Device>> GetByStatusAsync(DeviceStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Interfaces)
            .Where(d => d.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Device>> GetByTypeAsync(DeviceType type, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Interfaces)
            .Where(d => d.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Device>> GetByVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Interfaces)
            .Where(d => d.Vendor == vendor)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HostnameExistsAsync(string hostname, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AnyAsync(d => d.Hostname == hostname, cancellationToken);
    }

    public async Task<bool> IpAddressExistsAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AnyAsync(d => d.ManagementIp.Value == ipAddress, cancellationToken);
    }
}
