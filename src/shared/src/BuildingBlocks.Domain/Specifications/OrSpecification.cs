using System.Linq.Expressions;

namespace _116.BuildingBlocks.Domain.Specifications;

/// <summary>
/// Combines two specifications using logical OR.
/// </summary>
public class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <summary>
    /// Combines two specification expressions using logical OR.
    /// </summary>
    /// <returns>An expression that evaluates to true when either left or right specification is satisfied.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = left.ToExpression();
        Expression<Func<T, bool>> rightExpr = right.ToExpression();

        ParameterExpression param = Expression.Parameter(typeof(T));
        BinaryExpression body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
