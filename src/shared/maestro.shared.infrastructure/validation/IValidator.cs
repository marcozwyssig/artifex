using Maestro.Shared.Domain;

namespace Maestro.Shared.Infrastructure.Validation;

/// <summary>
/// Interface for validators
/// </summary>
public interface IValidator<in T>
{
    Task<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
