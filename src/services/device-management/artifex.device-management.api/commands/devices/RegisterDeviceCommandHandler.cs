using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.DeviceManagement.Domain.Repositories;
using Artifex.DeviceManagement.Domain.ValueObjects;
using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Infrastructure.Messaging;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Api.Commands;

/// <summary>
/// Handler for RegisterDeviceCommand
/// Uses abstracted IMessageBus instead of direct MassTransit dependency
/// </summary>
public class RegisterDeviceCommandHandler : ICommandHandler<RegisterDeviceCommand, Result<Guid>>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;

    public RegisterDeviceCommandHandler(
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
    }

    public async Task<Result<Guid>> HandleAsync(RegisterDeviceCommand command, CancellationToken cancellationToken = default)
    {
        // Check if hostname already exists
        if (await _deviceRepository.HostnameExistsAsync(command.Hostname, cancellationToken))
        {
            return Result.Failure<Guid>($"Device with hostname '{command.Hostname}' already exists");
        }

        // Check if IP address already exists
        if (await _deviceRepository.IpAddressExistsAsync(command.ManagementIp, cancellationToken))
        {
            return Result.Failure<Guid>($"Device with IP address '{command.ManagementIp}' already exists");
        }

        // Create value objects
        var ipAddressResult = IpAddress.Create(command.ManagementIp);
        if (ipAddressResult.IsFailure)
        {
            return Result.Failure<Guid>(ipAddressResult.Error);
        }

        var credentialsResult = Credentials.Create(command.Username, command.Password, command.EnablePassword);
        if (credentialsResult.IsFailure)
        {
            return Result.Failure<Guid>(credentialsResult.Error);
        }

        // Parse enums
        if (!Enum.TryParse<DeviceType>(command.DeviceType, true, out var deviceType))
        {
            return Result.Failure<Guid>($"Invalid device type: {command.DeviceType}");
        }

        if (!Enum.TryParse<Vendor>(command.Vendor, true, out var vendor))
        {
            return Result.Failure<Guid>($"Invalid vendor: {command.Vendor}");
        }

        // Create device
        var deviceResult = Device.Create(
            command.Hostname,
            ipAddressResult.Value!,
            deviceType,
            vendor,
            credentialsResult.Value!);

        if (deviceResult.IsFailure)
        {
            return Result.Failure<Guid>(deviceResult.Error);
        }

        var device = deviceResult.Value!;

        // Update optional fields
        if (!string.IsNullOrEmpty(command.Location) || !string.IsNullOrEmpty(command.Description))
        {
            var updateResult = device.UpdateInformation(
                location: command.Location,
                description: command.Description);

            if (updateResult.IsFailure)
            {
                return Result.Failure<Guid>(updateResult.Error);
            }
        }

        // Save device
        await _deviceRepository.SaveAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain events via abstracted message bus
        // Works with both MassTransit (production) and InMemory (development)
        foreach (var domainEvent in device.DomainEvents)
        {
            await _messageBus.PublishAsync(domainEvent, cancellationToken);
        }

        device.ClearDomainEvents();

        return Result.Success(device.Id);
    }
}
