using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maestro.DeviceManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Device entity
/// </summary>
public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Hostname)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(d => d.Hostname)
            .IsUnique();

        // Value object: IpAddress
        builder.OwnsOne(d => d.ManagementIp, ip =>
        {
            ip.Property(i => i.Value)
                .HasColumnName("ManagementIp")
                .IsRequired()
                .HasMaxLength(45); // IPv6 max length

            ip.HasIndex(i => i.Value)
                .IsUnique();
        });

        // Value object: MacAddress
        builder.OwnsOne(d => d.MacAddress, mac =>
        {
            mac.Property(m => m.Value)
                .HasColumnName("MacAddress")
                .HasMaxLength(17);
        });

        // Value object: Credentials (stored encrypted in production)
        builder.OwnsOne(d => d.Credentials, cred =>
        {
            cred.Property(c => c.Username)
                .HasColumnName("Username")
                .IsRequired()
                .HasMaxLength(100);

            cred.Property(c => c.Password)
                .HasColumnName("Password")
                .IsRequired()
                .HasMaxLength(500); // Encrypted password

            cred.Property(c => c.EnablePassword)
                .HasColumnName("EnablePassword")
                .HasMaxLength(500);
        });

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Vendor)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Model)
            .HasMaxLength(255);

        builder.Property(d => d.SerialNumber)
            .HasMaxLength(255);

        builder.Property(d => d.SoftwareVersion)
            .HasMaxLength(255);

        builder.Property(d => d.Location)
            .HasMaxLength(500);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.LastSeenAt);

        builder.Property(d => d.LastModifiedAt);

        // Relationships
        builder.HasMany<Interface>("_interfaces")
            .WithOne()
            .HasForeignKey(i => i.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (not persisted)
        builder.Ignore(d => d.DomainEvents);
    }
}
