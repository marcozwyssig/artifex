using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Domain;

namespace Artifex.Shared.Infrastructure.Validation;

/// <summary>
/// Validation behavior for command pipeline
/// </summary>
public class ValidationBehavior<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _handler;
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationBehavior(
        ICommandHandler<TCommand, TResult> handler,
        IEnumerable<IValidator<TCommand>> validators)
    {
        _handler = handler;
        _validators = validators;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var validationTasks = _validators
            .Select(v => v.ValidateAsync(command, cancellationToken));

        var validationResults = await Task.WhenAll(validationTasks);

        var failures = validationResults
            .Where(r => r.IsFailure)
            .Select(r => r.Error.ToString())
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await _handler.HandleAsync(command, cancellationToken);
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    public ValidationException(IReadOnlyCollection<string> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}
