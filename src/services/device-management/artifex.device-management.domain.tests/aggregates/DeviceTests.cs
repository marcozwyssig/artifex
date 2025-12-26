using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.DeviceManagement.Domain.Events;
using Artifex.DeviceManagement.Domain.ValueObjects;
using Xunit;

namespace Artifex.DeviceManagement.Domain.Tests.Aggregates;

public class DeviceTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var ipAddress = IpAddress.Create("192.168.1.1").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;

        // Act
        var result = Device.Create(
            "test-router",
            ipAddress,
            DeviceType.Router,
            Vendor.Cisco,
            credentials);

        // Assert
        Assert.True(result.IsSuccess);
        var device = result.Value!;
        Assert.Equal("test-router", device.Hostname);
        Assert.Equal(ipAddress, device.ManagementIp);
        Assert.Equal(DeviceType.Router, device.Type);
        Assert.Equal(Vendor.Cisco, device.Vendor);
        Assert.Equal(DeviceStatus.Unknown, device.Status);
        Assert.Single(device.DomainEvents);
        Assert.IsType<DeviceRegisteredEvent>(device.DomainEvents.First());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyHostname_ShouldFail(string hostname)
    {
        // Arrange
        var ipAddress = IpAddress.Create("192.168.1.1").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;

        // Act
        var result = Device.Create(
            hostname,
            ipAddress,
            DeviceType.Router,
            Vendor.Cisco,
            credentials);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithShortHostname_ShouldFail()
    {
        // Arrange
        var ipAddress = IpAddress.Create("192.168.1.1").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;

        // Act
        var result = Device.Create(
            "ab",
            ipAddress,
            DeviceType.Router,
            Vendor.Cisco,
            credentials);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("at least 3 characters", result.Errors.First());
    }

    [Fact]
    public void ChangeStatus_WithDifferentStatus_ShouldRaiseDomainEvent()
    {
        // Arrange
        var device = CreateValidDevice();
        device.ClearDomainEvents();

        // Act
        var result = device.ChangeStatus(DeviceStatus.Online);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DeviceStatus.Online, device.Status);
        Assert.Single(device.DomainEvents);
        Assert.IsType<DeviceStatusChangedEvent>(device.DomainEvents.First());
    }

    [Fact]
    public void ChangeStatus_WithSameStatus_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var device = CreateValidDevice();
        device.ClearDomainEvents();

        // Act
        var result = device.ChangeStatus(DeviceStatus.Unknown);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(device.DomainEvents);
    }

    [Fact]
    public void SetOnline_ShouldSetStatusAndUpdateLastSeen()
    {
        // Arrange
        var device = CreateValidDevice();
        var beforeLastSeen = device.LastSeenAt;

        // Act
        var result = device.SetOnline();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DeviceStatus.Online, device.Status);
        Assert.NotNull(device.LastSeenAt);
        Assert.NotEqual(beforeLastSeen, device.LastSeenAt);
    }

    [Fact]
    public void UpdateInformation_WithValidData_ShouldUpdate()
    {
        // Arrange
        var device = CreateValidDevice();

        // Act
        var result = device.UpdateInformation(
            hostname: "updated-router",
            model: "Cisco ISR 4431",
            location: "Data Center 1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("updated-router", device.Hostname);
        Assert.Equal("Cisco ISR 4431", device.Model);
        Assert.Equal("Data Center 1", device.Location);
    }

    [Fact]
    public void AddInterface_WithUniqueInterface_ShouldSucceed()
    {
        // Arrange
        var device = CreateValidDevice();
        var interfaceResult = Interface.Create(device.Id, "GigabitEthernet0/0", InterfaceType.GigabitEthernet);
        var @interface = interfaceResult.Value!;

        // Act
        var result = device.AddInterface(@interface);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(device.Interfaces);
    }

    [Fact]
    public void AddInterface_WithDuplicateName_ShouldFail()
    {
        // Arrange
        var device = CreateValidDevice();
        var interface1 = Interface.Create(device.Id, "GigabitEthernet0/0", InterfaceType.GigabitEthernet).Value!;
        var interface2 = Interface.Create(device.Id, "GigabitEthernet0/0", InterfaceType.GigabitEthernet).Value!;

        device.AddInterface(interface1);

        // Act
        var result = device.AddInterface(interface2);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Errors.First());
    }

    [Fact]
    public void MarkAsDeleted_ShouldRaiseDeletedEvent()
    {
        // Arrange
        var device = CreateValidDevice();
        device.ClearDomainEvents();

        // Act
        device.MarkAsDeleted();

        // Assert
        Assert.Single(device.DomainEvents);
        Assert.IsType<DeviceDeletedEvent>(device.DomainEvents.First());
    }

    private Device CreateValidDevice()
    {
        var ipAddress = IpAddress.Create("192.168.1.1").Value!;
        var credentials = Credentials.Create("admin", "password").Value!;
        return Device.Create(
            "test-router",
            ipAddress,
            DeviceType.Router,
            Vendor.Cisco,
            credentials).Value!;
    }
}
