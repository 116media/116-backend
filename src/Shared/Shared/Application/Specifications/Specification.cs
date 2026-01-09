using System.Linq.Expressions;

namespace _116.Shared.Application.Specifications;

/// <summary>
/// Base class for defining specifications and composing them using logical operators.
/// </summary>
/// <typeparam name="T">The type the specification applies to.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <inheritdoc />
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc />
    public bool IsSatisfiedBy(T entity)
    {
        Func<T, bool> predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Combines the current specification with another using logical AND.
    /// </summary>
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combines the current specification with another using logical OR.
    /// </summary>
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);

    /// <summary>
    /// Inverts the current specification using the logical NOT.
    /// </summary>
    public Specification<T> Not() => new NotSpecification<T>(this);

    /// <summary>
    /// Combines multiple specifications using logical AND.
    /// </summary>
    public static Specification<T> AndAll(params Specification<T>[] specifications)
    {
        if (specifications == null || specifications.Length == 0)
        {
            throw new ArgumentException("At least one specification is required.");
        }

        return specifications.Aggregate((current, next) => new AndSpecification<T>(current, next));
    }

    /// <summary>
    /// Combines multiple specifications using logical OR.
    /// </summary>
    public static Specification<T> OrAll(params Specification<T>[] specifications)
    {
        if (specifications == null || specifications.Length == 0)
        {
            throw new ArgumentException("At least one specification is required.");
        }

        return specifications.Aggregate((current, next) => new OrSpecification<T>(current, next));
    }
}
