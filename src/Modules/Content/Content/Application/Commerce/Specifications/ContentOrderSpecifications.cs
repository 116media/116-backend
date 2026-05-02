using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

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
/// Specification that matches content orders by customer name, email, or company.
/// Uses case-insensitive matching (ILIKE in PostgreSQL) for partial search.
/// </summary>
public class ContentOrderSearchSpecification(string search) : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return order =>
            EF.Functions.ILike(order.Customer.FullName, pattern)
            || EF.Functions.ILike(order.Customer.Email, pattern)
            || (order.Customer.Company != null && EF.Functions.ILike(order.Customer.Company, pattern));
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
/// Specification that matches content payments by their verification status.
/// </summary>
public class ContentPaymentByStatusSpecification(EnumPaymentStatus status) : Specification<ContentPaymentEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentPaymentEntity, bool>> ToExpression()
    {
        return payment => payment.Status == status;
    }
}

/// <summary>
/// Specification that matches content payments by payment method.
/// </summary>
public class ContentPaymentByMethodSpecification(EnumPaymentMethod method) : Specification<ContentPaymentEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentPaymentEntity, bool>> ToExpression()
    {
        return payment => payment.PaymentMethod == method;
    }
}

/// <summary>
/// Specification that matches content payments by customer information.
/// Uses case-insensitive matching (ILIKE in PostgreSQL) across customer name, email, and company.
/// </summary>
public class ContentPaymentSearchSpecification(string search) : Specification<ContentPaymentEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentPaymentEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return payment =>
            EF.Functions.ILike(payment.Order.Customer.FullName, pattern)
            || EF.Functions.ILike(payment.Order.Customer.Email, pattern)
            || (payment.Order.Customer.Company != null && EF.Functions.ILike(payment.Order.Customer.Company, pattern));
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
