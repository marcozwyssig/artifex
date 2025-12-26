using Artifex.DeviceManagement.Domain.Repositories;
using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Infrastructure.Messaging;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Api.Commands;

/// <summary>
/// Handler for DeleteDeviceCommand
/// Uses abstracted IMessageBus instead of direct dependency
/// </summary>
public class DeleteDeviceCommandHandler : ICommandHandler<DeleteDeviceCommand, Result>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;

    public DeleteDeviceCommandHandler(
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
    }

    public async Task<Result> HandleAsync(DeleteDeviceCommand command, CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdAsync(command.DeviceId, cancellationToken);
        if (device == null)
        {
            return Result.Failure($"Device with ID {command.DeviceId} not found");
        }

        device.MarkAsDeleted();

        await _deviceRepository.DeleteAsync(device, cancellationToken);
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
