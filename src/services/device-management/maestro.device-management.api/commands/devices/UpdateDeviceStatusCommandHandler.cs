using Maestro.DeviceManagement.Domain.Enums;
using Maestro.DeviceManagement.Domain.Repositories;
using Maestro.Shared.Infrastructure.Commands;
using Maestro.Shared.Infrastructure.Messaging;
using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Api.Commands;

/// <summary>
/// Handler for UpdateDeviceStatusCommand
/// Uses abstracted IMessageBus instead of direct dependency
/// </summary>
public class UpdateDeviceStatusCommandHandler : ICommandHandler<UpdateDeviceStatusCommand, Result>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;

    public UpdateDeviceStatusCommandHandler(
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
    }

    public async Task<Result> HandleAsync(UpdateDeviceStatusCommand command, CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdAsync(command.DeviceId, cancellationToken);
        if (device == null)
        {
            return Result.Failure($"Device with ID {command.DeviceId} not found");
        }

        if (!Enum.TryParse<DeviceStatus>(command.Status, true, out var status))
        {
            return Result.Failure($"Invalid device status: {command.Status}");
        }

        var result = device.ChangeStatus(status, command.Reason);
        if (result.IsFailure)
        {
            return result;
        }

        await _deviceRepository.UpdateAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain events via abstracted message bus
        foreach (var domainEvent in device.DomainEvents)
        {
            await _messageBus.PublishAsync(domainEvent, cancellationToken);
        }

        device.ClearDomainEvents();

        return Result.Success();
    }
}
