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

/// <summary>
/// Specification that matches a content payment by its associated order identifier.
/// </summary>
public class ContentPaymentByOrderIdSpecification(Guid orderId) : Specification<ContentPaymentEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentPaymentEntity, bool>> ToExpression()
    {
        return payment => payment.OrderId == orderId;
    }
}

/// <summary>
/// Specification that matches a content order item by its identifier within a specific order.
/// </summary>
public class ContentOrderItemByIdAndOrderIdSpecification(Guid orderId, Guid itemId)
    : Specification<ContentOrderItemEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderItemEntity, bool>> ToExpression()
    {
        return item => item.Id == itemId && item.OrderId == orderId;
    }
}
