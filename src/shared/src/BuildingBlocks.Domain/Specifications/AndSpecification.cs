using System.Linq.Expressions;

namespace _116.BuildingBlocks.Domain.Specifications;

/// <summary>
/// Combines two specifications using logical AND.
/// </summary>
public class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <summary>
    /// Combines two specification expressions using logical AND.
    /// </summary>
    /// <returns>An expression that evaluates to true when both left and right specifications are satisfied.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = left.ToExpression();
        Expression<Func<T, bool>> rightExpr = right.ToExpression();

        ParameterExpression param = Expression.Parameter(typeof(T));
        BinaryExpression body = Expression.AndAlso(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
