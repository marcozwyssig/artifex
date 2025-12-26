using Maestro.DeviceManagement.Domain.Roles;
using Xunit;

namespace Maestro.DeviceManagement.Domain.Tests.Roles;

public class ServerRoleTests
{
    [Fact]
    public void Create_WithValidConfiguration_ShouldSucceed()
    {
        // Act
        var result = ServerRole.Create(
            ServerType.WebServer,
            numberOfCpuCores: 8,
            memoryInGB: 16,
            services: new[] { "nginx", "apache" });

        // Assert
        Assert.True(result.IsSuccess);
        var role = result.Value!;
        Assert.Equal(ServerType.WebServer, role.Type);
        Assert.Equal(8, role.NumberOfCpuCores);
        Assert.Equal(16, role.MemoryInGB);
        Assert.Equal(2, role.Services.Count);
    }

    [Fact]
    public void Create_WithZeroCpuCores_ShouldFail()
    {
        // Act
        var result = ServerRole.Create(
            ServerType.ApplicationServer,
            numberOfCpuCores: 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("at least one CPU core", result.Errors.First());
    }

    [Fact]
    public void Create_WithZeroMemory_ShouldFail()
    {
        // Act
        var result = ServerRole.Create(
            ServerType.DatabaseServer,
            memoryInGB: 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("at least 1GB of memory", result.Errors.First());
    }

    [Fact]
    public void GetRequiredPorts_ForWebServer_ShouldIncludeWebPorts()
    {
        // Arrange
        var role = ServerRole.Create(ServerType.WebServer).Value!;

        // Act
        var ports = role.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports);  // SSH
        Assert.Contains(80, ports);  // HTTP
        Assert.Contains(443, ports); // HTTPS
    }

    [Fact]
    public void GetRequiredPorts_ForDatabaseServer_ShouldIncludeDatabasePorts()
    {
        // Arrange
        var role = ServerRole.Create(ServerType.DatabaseServer).Value!;

        // Act
        var ports = role.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports);    // SSH
        Assert.Contains(3306, ports);  // MySQL
        Assert.Contains(5432, ports);  // PostgreSQL
    }

    [Fact]
    public void GetRequiredPorts_ForDnsServer_ShouldIncludeDnsPort()
    {
        // Arrange
        var role = ServerRole.Create(ServerType.DnsServer).Value!;

        // Act
        var ports = role.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports); // SSH
        Assert.Contains(53, ports); // DNS
    }

    [Fact]
    public void Create_WithHighAvailability_ShouldSetProperty()
    {
        // Act
        var result = ServerRole.Create(
            ServerType.DatabaseServer,
            isHighAvailability: true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsHighAvailability);
    }

    [Fact]
    public void Create_AsVirtualized_ShouldSetProperty()
    {
        // Act
        var result = ServerRole.Create(
            ServerType.ApplicationServer,
            isVirtualized: true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsVirtualized);
    }
}
