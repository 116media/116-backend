using System.Linq.Expressions;

namespace _116.BuildingBlocks.Domain.Specifications;

/// <summary>
/// Represents a specification pattern that defines filtering criteria for objects of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of object the specification applies to.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Converts the specification into a LINQ expression.
    /// </summary>
    /// <returns>An expression that represents the filtering criteria.</returns>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Evaluates whether a given object satisfies the specification.
    /// </summary>
    /// <param name="entity">The object to evaluate.</param>
    /// <returns><c>true</c> if the object satisfies the specification; otherwise, <c>false</c>.</returns>
    bool IsSatisfiedBy(T entity);
}
