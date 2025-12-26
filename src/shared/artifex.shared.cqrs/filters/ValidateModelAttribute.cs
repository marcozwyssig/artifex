using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Artifex.Shared.Ui.Filters;

/// <summary>
/// Action filter for automatic model validation
/// </summary>
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                status = 400,
                message = "Validation failed",
                errors
            });
        }
    }
}
