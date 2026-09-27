using System.Linq.Expressions;
using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Catalog.Specifications;

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
