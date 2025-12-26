namespace Artifex.Shared.Infrastructure.Logging;

/// <summary>
/// Logger adapter interface for application-level logging abstraction
/// </summary>
public interface ILoggerAdapter<T>
{
    void LogTrace(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogCritical(Exception exception, string message, params object[] args);
}
