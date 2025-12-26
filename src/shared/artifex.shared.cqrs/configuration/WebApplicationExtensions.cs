using Artifex.Shared.Ui.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Artifex.Shared.Ui.Configuration;

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
    /// Adds all shared ui middleware
    /// </summary>
    public static IApplicationBuilder UseSharedUi(this IApplicationBuilder app)
    {
        app.UseRequestLogging();
        app.UseGlobalExceptionHandling();
        return app;
    }
}
