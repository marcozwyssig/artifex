using System.Linq.Expressions;

namespace Maestro.Shared.Domain.Specifications;

/// <summary>
/// Specification pattern for encapsulating query logic
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Converts the specification to a LINQ expression
    /// </summary>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Checks if an entity satisfies the specification
    /// </summary>
    bool IsSatisfiedBy(T entity);
}
