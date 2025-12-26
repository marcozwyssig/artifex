using Artifex.DeviceManagement.Domain.Roles;
using Xunit;

namespace Artifex.DeviceManagement.Domain.Tests.Roles;

public class RouterRoleTests
{
    [Fact]
    public void Create_WithValidConfiguration_ShouldSucceed()
    {
        // Act
        var result = RouterRole.Create(
            isEdgeRouter: true,
            supportsBGP: true,
            supportsOSPF: true);

        // Assert
        Assert.True(result.IsSuccess);
        var role = result.Value!;
        Assert.True(role.IsEdgeRouter);
        Assert.True(role.SupportsBGP);
        Assert.True(role.SupportsOSPF);
    }

    [Fact]
    public void Create_WithBothEdgeAndCore_ShouldFail()
    {
        // Act
        var result = RouterRole.Create(
            isEdgeRouter: true,
            isCoreRouter: true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("cannot be both edge and core", result.Errors.First());
    }

    [Fact]
    public void Create_WithZeroRoutingTables_ShouldFail()
    {
        // Act
        var result = RouterRole.Create(numberOfRoutingTables: 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("at least one routing table", result.Errors.First());
    }

    [Fact]
    public void GetRequiredPorts_WithBGP_ShouldIncludeBGPPort()
    {
        // Arrange
        var role = RouterRole.Create(supportsBGP: true).Value!;

        // Act
        var ports = role.GetRequiredPorts();

        // Assert
        Assert.Contains(179, ports); // BGP port
        Assert.Contains(22, ports);  // SSH
        Assert.Contains(161, ports); // SNMP
    }

    [Fact]
    public void GetRequiredPorts_WithoutBGP_ShouldNotIncludeBGPPort()
    {
        // Arrange
        var role = RouterRole.Create(supportsBGP: false).Value!;

        // Act
        var ports = role.GetRequiredPorts();

        // Assert
        Assert.DoesNotContain(179, ports);
        Assert.Contains(22, ports);
        Assert.Contains(161, ports);
    }
}
