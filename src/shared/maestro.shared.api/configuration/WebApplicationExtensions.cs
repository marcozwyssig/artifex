using Maestro.Shared.Presentation.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Maestro.Shared.Presentation.Configuration;

/// <summary>
/// Extension methods for configuring ASP.NET Core Web Application
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds exception handling middleware
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    /// <summary>
    /// Adds request logging middleware
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }

    /// <summary>
    /// Adds all shared presentation middleware
    /// </summary>
    public static IApplicationBuilder UseSharedPresentation(this IApplicationBuilder app)
    {
        app.UseRequestLogging();
        app.UseGlobalExceptionHandling();
        return app;
    }
}
