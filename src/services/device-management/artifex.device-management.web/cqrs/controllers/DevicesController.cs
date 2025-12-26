using Artifex.DeviceManagement.Application.Commands;
using Artifex.DeviceManagement.Application.DTOs;
using Artifex.DeviceManagement.Application.Queries;
using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Cqrs.DTOs;
using Artifex.Shared.Cqrs.Queries;
using Artifex.Shared.Domain;
using Artifex.Shared.Ui.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Artifex.DeviceManagement.Ui.Cqrs.Controllers;

/// <summary>
/// API controller for device management operations
/// </summary>
[Route("api/devices")]
public class DevicesController : BaseCqrsController
{
    private readonly ICommandHandler<RegisterDeviceCommand, Result<Guid>> _registerDeviceHandler;
    private readonly ICommandHandler<UpdateDeviceStatusCommand, Result> _updateStatusHandler;
    private readonly ICommandHandler<DeleteDeviceCommand, Result> _deleteDeviceHandler;
    private readonly IQueryHandler<GetDeviceByIdQuery, Result<DeviceDto>> _getDeviceByIdHandler;
    private readonly IQueryHandler<GetAllDevicesQuery, PagedResult<DeviceDto>> _getAllDevicesHandler;

    public DevicesController(
        ICommandHandler<RegisterDeviceCommand, Result<Guid>> registerDeviceHandler,
        ICommandHandler<UpdateDeviceStatusCommand, Result> updateStatusHandler,
        ICommandHandler<DeleteDeviceCommand, Result> deleteDeviceHandler,
        IQueryHandler<GetDeviceByIdQuery, Result<DeviceDto>> getDeviceByIdHandler,
        IQueryHandler<GetAllDevicesQuery, PagedResult<DeviceDto>> getAllDevicesHandler)
    {
        _registerDeviceHandler = registerDeviceHandler;
        _updateStatusHandler = updateStatusHandler;
        _deleteDeviceHandler = deleteDeviceHandler;
        _getDeviceByIdHandler = getDeviceByIdHandler;
        _getAllDevicesHandler = getAllDevicesHandler;
    }

    /// <summary>
    /// Get all devices with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DeviceDto>), 200)]
    public async Task<ActionResult<PagedResult<DeviceDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] string? vendor = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllDevicesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            Type = type,
            Vendor = vendor,
            SearchTerm = searchTerm
        };

        var result = await _getAllDevicesHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a device by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DeviceDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetDeviceByIdQuery { DeviceId = id };
        var result = await _getDeviceByIdHandler.HandleAsync(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Register a new device
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Guid>> Register(
        [FromBody] RegisterDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _registerDeviceHandler.HandleAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update device status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateDeviceStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateDeviceStatusCommand
        {
            DeviceId = id,
            Status = request.Status,
            Reason = request.Reason
        };

        var result = await _updateStatusHandler.HandleAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Delete a device
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteDeviceCommand { DeviceId = id };
        var result = await _deleteDeviceHandler.HandleAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleResult(result);
    }
}

/// <summary>
/// Request model for updating device status
/// </summary>
public class UpdateDeviceStatusRequest
{
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
