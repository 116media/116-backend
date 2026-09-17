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
/// Specification that matches content orders by customer name, email, or company, probing the
/// injected customer set (ILIKE in PostgreSQL) so the order carries no customer navigation.
/// </summary>
public class ContentOrderSearchSpecification(string search, IQueryable<CustomerEntity> customers)
    : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return order =>
            customers.Any(customer =>
                customer.Id == order.CustomerId
                && (
                    EF.Functions.ILike(customer.FullName, pattern)
                    || EF.Functions.ILike(customer.Email, pattern)
                    || (customer.Company != null && EF.Functions.ILike(customer.Company, pattern))
                )
            );
    }
}

/// <summary>
/// Specification that matches content orders carrying a payment record — the root the admin
/// payments listing pages over.
/// </summary>
public class OrderHasPaymentSpecification : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        return order => order.Payment != null;
    }
}

/// <summary>
/// Specification that matches content orders whose payment has the given verification status.
/// </summary>
public class OrderPaymentByStatusSpecification(EnumPaymentStatus status) : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        return order => order.Payment != null && order.Payment.Status == status;
    }
}

/// <summary>
/// Specification that matches content orders whose payment uses the given payment method.
/// </summary>
public class OrderPaymentByMethodSpecification(EnumPaymentMethod method) : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        return order => order.Payment != null && order.Payment.PaymentMethod == method;
    }
}

/// <summary>
/// Specification that matches the order owning a given item.
/// </summary>
public class ContentOrderByItemIdSpecification(Guid orderItemId) : Specification<ContentOrderEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentOrderEntity, bool>> ToExpression()
    {
        return order => order.Items.Any(item => item.Id == orderItemId);
    }
}
