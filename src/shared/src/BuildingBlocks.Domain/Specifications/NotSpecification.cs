using System.Linq.Expressions;

namespace _116.BuildingBlocks.Domain.Specifications;

/// <summary>
/// Inverts a specification using the logical NOT.
/// </summary>
public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotSpecification{T}"/> class.
    /// </summary>
    /// <param name="inner">The specification to negate.</param>
    public NotSpecification(Specification<T> inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Inverts the inner specification expression using logical NOT.
    /// </summary>
    /// <returns>An expression that evaluates to true when the inner specification is not satisfied.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> innerExpr = _inner.ToExpression();
        ParameterExpression param = Expression.Parameter(typeof(T));
        UnaryExpression body = Expression.Not(Expression.Invoke(innerExpr, param));

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
