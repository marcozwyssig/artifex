using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.DeviceManagement.Domain.Events;
using Artifex.DeviceManagement.Domain.Repositories;
using Artifex.DeviceManagement.Domain.Services;
using Artifex.DeviceManagement.Domain.ValueObjects;
using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Infrastructure.Messaging;
using Artifex.Shared.Domain;
using Microsoft.Extensions.Logging;

namespace Artifex.DeviceManagement.Api.Commands;

/// <summary>
/// Handler for DiscoverDevicesCommand
/// Uses abstracted IMessageBus instead of direct dependency
/// </summary>
public class DiscoverDevicesCommandHandler : ICommandHandler<DiscoverDevicesCommand, Result<DeviceDiscoveryResult>>
{
    private readonly IDeviceDiscoveryService _discoveryService;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<DiscoverDevicesCommandHandler> _logger;

    public DiscoverDevicesCommandHandler(
        IDeviceDiscoveryService discoveryService,
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
        ILogger<DiscoverDevicesCommandHandler> logger)
    {
        _discoveryService = discoveryService;
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<Result<DeviceDiscoveryResult>> HandleAsync(
        DiscoverDevicesCommand command,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("Starting device discovery for network {NetworkCidr}", command.NetworkCidr);

        // Validate CIDR
        if (string.IsNullOrWhiteSpace(command.NetworkCidr))
        {
            return Result.Failure<DeviceDiscoveryResult>("Network CIDR cannot be empty");
        }

        // Discover devices
        var discoveryResult = await _discoveryService.DiscoverDevicesAsync(command.NetworkCidr, cancellationToken);

        if (discoveryResult.IsFailure)
        {
            return Result.Failure<DeviceDiscoveryResult>(discoveryResult.Error);
        }

        var discoveredDevices = discoveryResult.Value!;
        var devicesRegistered = 0;

        // Publish discovery events via abstracted message bus
        foreach (var discoveredDevice in discoveredDevices)
        {
            var discoveryEvent = new DeviceDiscoveredEvent(
                discoveredDevice.IpAddress,
                discoveredDevice.MacAddress,
                discoveredDevice.Hostname,
                discoveredDevice.Vendor,
                discoveredDevice.DeviceType,
                discoveredDevice.IsReachable);

            await _messageBus.PublishAsync(discoveryEvent, cancellationToken);
        }

        // Auto-register devices if requested
        if (command.AutoRegister)
        {
            devicesRegistered = await AutoRegisterDevicesAsync(
                discoveredDevices,
                command.DefaultUsername,
                command.DefaultPassword,
                cancellationToken);
        }

        var endTime = DateTime.UtcNow;

        var result = new DeviceDiscoveryResult
        {
            TotalScanned = GetEstimatedScanSize(command.NetworkCidr),
            DevicesFound = discoveredDevices.Count,
            DevicesRegistered = devicesRegistered,
            Devices = discoveredDevices,
            DiscoveryStartedAt = startTime,
            DiscoveryCompletedAt = endTime
        };

        _logger.LogInformation(
            "Device discovery completed. Found: {Found}, Registered: {Registered}, Duration: {Duration}ms",
            result.DevicesFound,
            result.DevicesRegistered,
            result.Duration.TotalMilliseconds);

        return Result<DeviceDiscoveryResult>.Success(result);
    }

    private async Task<int> AutoRegisterDevicesAsync(
        IReadOnlyCollection<DiscoveredDevice> discoveredDevices,
        string? defaultUsername,
        string? defaultPassword,
        CancellationToken cancellationToken)
    {
        var registered = 0;

        foreach (var discoveredDevice in discoveredDevices.Where(d => d.IsReachable))
        {
            try
            {
                // Check if device already exists
                var existingDevice = await _deviceRepository.GetByIpAddressAsync(
                    discoveredDevice.IpAddress,
                    cancellationToken);

                if (existingDevice != null)
                {
                    _logger.LogDebug("Device {IpAddress} already registered, skipping", discoveredDevice.IpAddress);
                    continue;
                }

                // Create device from discovered information
                var registrationResult = await TryRegisterDiscoveredDeviceAsync(
                    discoveredDevice,
                    defaultUsername,
                    defaultPassword,
                    cancellationToken);

                if (registrationResult.IsSuccess)
                {
                    registered++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-register device {IpAddress}", discoveredDevice.IpAddress);
            }
        }

        return registered;
    }

    private async Task<Result> TryRegisterDiscoveredDeviceAsync(
        DiscoveredDevice discoveredDevice,
        string? defaultUsername,
        string? defaultPassword,
        CancellationToken cancellationToken)
    {
        // Parse IP address
        var ipAddressResult = IpAddress.Create(discoveredDevice.IpAddress);
        if (ipAddressResult.IsFailure)
        {
            return ipAddressResult;
        }

        // Determine device type
        if (!Enum.TryParse<DeviceType>(discoveredDevice.DeviceType, true, out var deviceType))
        {
            deviceType = DeviceType.Unknown;
        }

        // Determine vendor
        if (!Enum.TryParse<Vendor>(discoveredDevice.Vendor, true, out var vendor))
        {
            vendor = Vendor.Unknown;
        }

        // Create default credentials if provided
        if (string.IsNullOrEmpty(defaultUsername) || string.IsNullOrEmpty(defaultPassword))
        {
            return Result.Failure("Default credentials required for auto-registration");
        }

        var credentialsResult = Credentials.Create(defaultUsername, defaultPassword);
        if (credentialsResult.IsFailure)
        {
            return credentialsResult;
        }

        // Create device
        var hostname = discoveredDevice.Hostname ?? $"device-{discoveredDevice.IpAddress.Replace(".", "-")}";

        var deviceResult = Device.Create(
            hostname,
            ipAddressResult.Value!,
            deviceType,
            vendor,
            credentialsResult.Value!,
            NetworkSegment.Unknown); // Will need to be set manually

        if (deviceResult.IsFailure)
        {
            return deviceResult;
        }

        var device = deviceResult.Value!;

        // Update optional information
        if (!string.IsNullOrEmpty(discoveredDevice.Model))
        {
            device.UpdateInformation(model: discoveredDevice.Model);
        }

        if (!string.IsNullOrEmpty(discoveredDevice.SoftwareVersion))
        {
            device.UpdateInformation(softwareVersion: discoveredDevice.SoftwareVersion);
        }

        // Set status as discovering
        device.ChangeStatus(DeviceStatus.Discovering, "Auto-discovered device");

        // Save device
        await _deviceRepository.SaveAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish events via abstracted message bus
        foreach (var domainEvent in device.DomainEvents)
        {
            await _messageBus.PublishAsync(domainEvent, cancellationToken);
        }

        device.ClearDomainEvents();

        _logger.LogInformation("Auto-registered device {Hostname} at {IpAddress}", hostname, discoveredDevice.IpAddress);

        return Result.Success();
    }

    private int GetEstimatedScanSize(string networkCidr)
    {
        var parts = networkCidr.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var prefixLength))
        {
            return 0;
        }

        var hostBits = 32 - prefixLength;
        var numHosts = (int)Math.Pow(2, hostBits) - 2; // Exclude network and broadcast

        // Match the scanner's limit
        return Math.Min(numHosts, 1024);
    }
}
