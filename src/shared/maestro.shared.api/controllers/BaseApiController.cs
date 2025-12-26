using Maestro.Shared.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Maestro.Shared.Presentation.Controllers;

/// <summary>
/// Base controller for all API controllers with common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Maps a Result to an ActionResult
    /// </summary>
    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.Error.FirstOrDefault() switch
        {
            var error when result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase) == true => NotFound(new { errors = result.Error }),
            var error when result.Error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) == true => Unauthorized(new { errors = result.Error }),
            var error when result.Error.Contains("forbidden", StringComparison.OrdinalIgnoreCase) == true => Forbid(),
            _ => BadRequest(new { errors = result.Error })
        };
    }

    /// <summary>
    /// Maps a Result with value to an ActionResult
    /// </summary>
    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.FirstOrDefault() switch
        {
            var error when result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase) == true => NotFound(new { errors = result.Error }),
            var error when result.Error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) == true => Unauthorized(new { errors = result.Error }),
            var error when result.Error.Contains("forbidden", StringComparison.OrdinalIgnoreCase) == true => Forbid(),
            _ => BadRequest(new { errors = result.Error })
        };
    }

    /// <summary>
    /// Returns a created response with location
    /// </summary>
    protected ActionResult<T> CreatedResult<T>(string actionName, object routeValues, T value)
    {
        return CreatedAtAction(actionName, routeValues, value);
    }

    /// <summary>
    /// Returns a no content response
    /// </summary>
    protected ActionResult NoContentResult()
    {
        return NoContent();
    }
}
