using Artifex.DeviceManagement.Domain.Roles;
using Xunit;

namespace Artifex.DeviceManagement.Domain.Tests.Roles;

public class SwitchRoleTests
{
    [Fact]
    public void Create_WithValidConfiguration_ShouldSucceed()
    {
        // Act
        var result = SwitchRole.Create(
            isAccessSwitch: true,
            numberOfPorts: 48,
            numberOfVlans: 10);

        // Assert
        Assert.True(result.IsSuccess);
        var role = result.Value!;
        Assert.True(role.IsAccessSwitch);
        Assert.Equal(48, role.NumberOfPorts);
        Assert.Equal(10, role.NumberOfVlans);
    }

    [Fact]
    public void Create_WithMultipleHierarchicalRoles_ShouldFail()
    {
        // Act
        var result = SwitchRole.Create(
            isAccessSwitch: true,
            isDistributionSwitch: true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("only have one hierarchical role", result.Errors.First());
    }

    [Fact]
    public void Create_WithZeroPorts_ShouldFail()
    {
        // Act
        var result = SwitchRole.Create(numberOfPorts: 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("at least one port", result.Errors.First());
    }

    [Fact]
    public void Create_WithZeroVlans_ShouldFail()
    {
        // Act
        var result = SwitchRole.Create(numberOfVlans: 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("at least one VLAN", result.Errors.First());
    }

    [Fact]
    public void GetRequiredPorts_ShouldIncludeManagementPorts()
    {
        // Arrange
        var role = SwitchRole.Create().Value!;

        // Act
        var ports = role.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports);  // SSH
        Assert.Contains(80, ports);  // HTTP
        Assert.Contains(161, ports); // SNMP
        Assert.Contains(443, ports); // HTTPS
    }
}
