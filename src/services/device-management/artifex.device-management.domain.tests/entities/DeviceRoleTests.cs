using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.DeviceManagement.Domain.Roles;
using Artifex.DeviceManagement.Domain.ValueObjects;
using Xunit;

namespace Artifex.DeviceManagement.Domain.Tests.Entities;

public class DeviceRoleTests
{
    [Fact]
    public void SetRole_WithValidRouterRole_ShouldSucceed()
    {
        // Arrange
        var device = CreateValidRouter();
        var role = RouterRole.Create(
            isEdgeRouter: true,
            supportsBGP: true,
            supportsOSPF: true).Value!;

        // Act
        var result = device.SetRole(role);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(device.Role);
        Assert.IsType<RouterRole>(device.Role);
    }

    [Fact]
    public void SetRole_WithValidSwitchRole_ShouldSucceed()
    {
        // Arrange
        var device = CreateValidSwitch();
        var role = SwitchRole.Create(
            isAccessSwitch: true,
            numberOfPorts: 48).Value!;

        // Act
        var result = device.SetRole(role);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(device.Role);
        Assert.IsType<SwitchRole>(device.Role);
    }

    [Fact]
    public void SetRole_WithValidServerRole_ShouldSucceed()
    {
        // Arrange
        var device = CreateValidServer();
        var role = ServerRole.Create(
            ServerType.WebServer,
            numberOfCpuCores: 8,
            memoryInGB: 16).Value!;

        // Act
        var result = device.SetRole(role);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(device.Role);
        Assert.IsType<ServerRole>(device.Role);
    }

    [Fact]
    public void SetRole_WithInvalidRole_ShouldFail()
    {
        // Arrange
        var device = CreateValidRouter();
        var invalidRole = RouterRole.Create(
            isEdgeRouter: true,
            isCoreRouter: true); // Invalid: both edge and core

        // Act
        var result = device.SetRole(invalidRole.Value!);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UpdateNetworkSegment_WithValidSegment_ShouldSucceed()
    {
        // Arrange
        var device = CreateValidRouter();

        // Act
        var result = device.UpdateNetworkSegment(NetworkSegment.WAN);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(NetworkSegment.WAN, device.NetworkSegment);
    }

    [Fact]
    public void GetRequiredPorts_WithRouterRole_ShouldReturnRouterPorts()
    {
        // Arrange
        var device = CreateValidRouter();
        var role = RouterRole.Create(supportsBGP: true).Value!;
        device.SetRole(role);

        // Act
        var ports = device.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports);  // SSH
        Assert.Contains(161, ports); // SNMP
        Assert.Contains(179, ports); // BGP
    }

    [Fact]
    public void GetRequiredPorts_WithSwitchRole_ShouldReturnSwitchPorts()
    {
        // Arrange
        var device = CreateValidSwitch();
        var role = SwitchRole.Create().Value!;
        device.SetRole(role);

        // Act
        var ports = device.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports);  // SSH
        Assert.Contains(80, ports);  // HTTP
        Assert.Contains(161, ports); // SNMP
        Assert.Contains(443, ports); // HTTPS
    }

    [Fact]
    public void GetRequiredPorts_WithServerRole_ShouldReturnServerPorts()
    {
        // Arrange
        var device = CreateValidServer();
        var role = ServerRole.Create(ServerType.WebServer).Value!;
        device.SetRole(role);

        // Act
        var ports = device.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports);  // SSH
        Assert.Contains(80, ports);  // HTTP
        Assert.Contains(443, ports); // HTTPS
    }

    [Fact]
    public void GetRequiredPorts_WithoutRole_ShouldReturnDefaultPorts()
    {
        // Arrange
        var device = CreateValidRouter();

        // Act
        var ports = device.GetRequiredPorts();

        // Assert
        Assert.Contains(22, ports);  // SSH
        Assert.Contains(161, ports); // SNMP
        Assert.Equal(2, ports.Count);
    }

    [Fact]
    public void Create_WithNetworkSegment_ShouldSetSegment()
    {
        // Arrange
        var ipAddress = IpAddress.Create("192.168.1.1").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;

        // Act
        var result = Device.Create(
            "wan-router",
            ipAddress,
            DeviceType.Router,
            Vendor.Cisco,
            credentials,
            NetworkSegment.WAN);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(NetworkSegment.WAN, result.Value!.NetworkSegment);
    }

    private Device CreateValidRouter()
    {
        var ipAddress = IpAddress.Create("192.168.1.1").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;
        return Device.Create(
            "test-router",
            ipAddress,
            DeviceType.Router,
            Vendor.Cisco,
            credentials,
            NetworkSegment.LAN).Value!;
    }

    private Device CreateValidSwitch()
    {
        var ipAddress = IpAddress.Create("192.168.1.2").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;
        return Device.Create(
            "test-switch",
            ipAddress,
            DeviceType.Switch,
            Vendor.Cisco,
            credentials,
            NetworkSegment.LAN).Value!;
    }

    private Device CreateValidServer()
    {
        var ipAddress = IpAddress.Create("192.168.1.100").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;
        return Device.Create(
            "test-server",
            ipAddress,
            DeviceType.Server,
            Vendor.Dell,
            credentials,
            NetworkSegment.LAN).Value!;
    }
}
