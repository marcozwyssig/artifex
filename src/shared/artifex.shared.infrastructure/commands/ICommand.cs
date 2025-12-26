using Artifex.Shared.Domain;

namespace Artifex.Shared.Infrastructure.Commands;

/// <summary>
/// Marker interface for commands that don't return a value
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Interface for commands that return a result
/// </summary>
public interface ICommand<out TResult>
{
}
