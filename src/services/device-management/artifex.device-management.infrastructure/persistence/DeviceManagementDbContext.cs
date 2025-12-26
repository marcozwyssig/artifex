using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Artifex.DeviceManagement.Infrastructure.Persistence;

/// <summary>
/// Entity Framework DbContext for Device Management
/// </summary>
public class DeviceManagementDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Interface> Interfaces => Set<Interface>();

    public DeviceManagementDbContext(DbContextOptions<DeviceManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new InterfaceConfiguration());
    }
}
