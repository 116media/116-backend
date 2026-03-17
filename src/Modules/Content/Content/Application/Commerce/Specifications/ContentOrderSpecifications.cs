using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Specifications;

/// <summary>
/// Specification that matches a content order by its unique identifier.
/// </summary>
public class ContentOrderByIdSpecification(Guid id) : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        return order => order.Id == id;
    }
}

/// <summary>
/// Specification that matches content orders by their lifecycle status.
/// </summary>
public class ContentOrderByStatusSpecification(EnumOrderStatus status) : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        return order => order.Status == status;
    }
}

/// <summary>
/// Specification that matches content orders placed by a specific customer.
/// </summary>
public class ContentOrderByCustomerIdSpecification(Guid customerId) : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        return order => order.CustomerId == customerId;
    }
}
