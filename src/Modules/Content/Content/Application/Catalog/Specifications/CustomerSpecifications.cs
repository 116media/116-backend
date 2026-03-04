using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Catalog.Specifications;

/// <summary>
/// Specification that matches a customer by its unique identifier.
/// </summary>
public class CustomerByIdSpecification(Guid id) : Specification<CustomerEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CustomerEntity, bool>> ToExpression()
    {
        return customer => customer.Id == id;
    }
}

/// <summary>
/// Specification that matches a customer by email address (case-insensitive).
/// </summary>
public class CustomerByEmailSpecification(string email) : Specification<CustomerEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CustomerEntity, bool>> ToExpression()
    {
        return customer => EF.Functions.ILike(customer.Email, email);
    }
}
