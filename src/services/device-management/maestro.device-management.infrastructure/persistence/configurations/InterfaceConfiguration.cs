using Maestro.DeviceManagement.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maestro.DeviceManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Interface entity
/// </summary>
public class InterfaceConfiguration : IEntityTypeConfiguration<Interface>
{
    public void Configure(EntityTypeBuilder<Interface> builder)
    {
        builder.ToTable("Interfaces");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.DeviceId)
            .IsRequired();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(i => new { i.DeviceId, i.Name })
            .IsUnique();

        builder.Property(i => i.Description)
            .HasMaxLength(500);

        // Value object: IpAddress
        builder.OwnsOne(i => i.IpAddress, ip =>
        {
            ip.Property(a => a.Value)
                .HasColumnName("IpAddress")
                .HasMaxLength(45);
        });

        // Value object: MacAddress
        builder.OwnsOne(i => i.MacAddress, mac =>
        {
            mac.Property(m => m.Value)
                .HasColumnName("MacAddress")
                .HasMaxLength(17);
        });

        builder.Property(i => i.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Speed);

        builder.Property(i => i.Mtu);

        builder.Property(i => i.Vlan);

        builder.Property(i => i.IsManagement)
            .IsRequired();

        builder.Property(i => i.LastStatusChange);
    }
}
