using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.Enums;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Repositories;

/// <summary>
/// Repository interface for Device aggregate
/// </summary>
public interface IDeviceRepository : IRepository<Device, Guid>
{
    Task<Device?> GetByHostnameAsync(string hostname, CancellationToken cancellationToken = default);
    Task<Device?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Device>> GetByStatusAsync(DeviceStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Device>> GetByTypeAsync(DeviceType type, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Device>> GetByVendorAsync(Vendor vendor, CancellationToken cancellationToken = default);
    Task<bool> HostnameExistsAsync(string hostname, CancellationToken cancellationToken = default);
    Task<bool> IpAddressExistsAsync(string ipAddress, CancellationToken cancellationToken = default);
}
