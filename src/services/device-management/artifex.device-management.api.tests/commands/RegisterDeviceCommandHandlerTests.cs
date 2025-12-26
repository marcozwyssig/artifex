using Artifex.DeviceManagement.Api.Commands;
using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Repositories;
using Artifex.Shared.Domain;
using Artifex.Shared.Infrastructure.Messaging;
using Moq;
using Xunit;

namespace Artifex.DeviceManagement.Api.Tests.Commands;

public class RegisterDeviceCommandHandlerTests
{
    private readonly Mock<IDeviceRepository> _deviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly RegisterDeviceCommandHandler _handler;

    public RegisterDeviceCommandHandlerTests()
    {
        _deviceRepositoryMock = new Mock<IDeviceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _messageBusMock = new Mock<IMessageBus>();

        _handler = new RegisterDeviceCommandHandler(
            _deviceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _messageBusMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterDevice()
    {
        // Arrange
        var command = new RegisterDeviceCommand
        {
            Hostname = "test-router",
            ManagementIp = "192.168.1.1",
            DeviceType = "Router",
            Vendor = "Cisco",
            Username = "admin",
            Password = "password"
        };

        _deviceRepositoryMock
            .Setup(r => r.HostnameExistsAsync(command.Hostname, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _deviceRepositoryMock
            .Setup(r => r.IpAddressExistsAsync(command.ManagementIp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        _deviceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _messageBusMock.Verify(e => e.PublishAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingHostname_ShouldFail()
    {
        // Arrange
        var command = new RegisterDeviceCommand
        {
            Hostname = "existing-router",
            ManagementIp = "192.168.1.1",
            DeviceType = "Router",
            Vendor = "Cisco",
            Username = "admin",
            Password = "password"
        };

        _deviceRepositoryMock
            .Setup(r => r.HostnameExistsAsync(command.Hostname, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Errors.First());

        _deviceRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingIpAddress_ShouldFail()
    {
        // Arrange
        var command = new RegisterDeviceCommand
        {
            Hostname = "new-router",
            ManagementIp = "192.168.1.1",
            DeviceType = "Router",
            Vendor = "Cisco",
            Username = "admin",
            Password = "password"
        };

        _deviceRepositoryMock
            .Setup(r => r.HostnameExistsAsync(command.Hostname, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _deviceRepositoryMock
            .Setup(r => r.IpAddressExistsAsync(command.ManagementIp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Errors.First());
    }

    [Fact]
    public async Task Handle_WithInvalidIpAddress_ShouldFail()
    {
        // Arrange
        var command = new RegisterDeviceCommand
        {
            Hostname = "test-router",
            ManagementIp = "invalid-ip",
            DeviceType = "Router",
            Vendor = "Cisco",
            Username = "admin",
            Password = "password"
        };

        _deviceRepositoryMock
            .Setup(r => r.HostnameExistsAsync(command.Hostname, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _deviceRepositoryMock
            .Setup(r => r.IpAddressExistsAsync(command.ManagementIp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid IP address", result.Errors.First());
    }
}
