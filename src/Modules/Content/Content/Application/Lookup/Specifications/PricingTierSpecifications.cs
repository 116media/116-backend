using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Lookup.Specifications;

/// <summary>
/// Specification that matches a pricing tier by its unique identifier.
/// </summary>
public class PricingTierByIdSpecification(Guid id) : Specification<PricingTierEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PricingTierEntity, bool>> ToExpression()
    {
        return pricingTier => pricingTier.Id == id;
    }
}

/// <summary>
/// Specification that matches a pricing tier by its name (case-insensitive).
/// </summary>
public class PricingTierByNameSpecification(string name) : Specification<PricingTierEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PricingTierEntity, bool>> ToExpression()
    {
        return pricingTier => EF.Functions.ILike(pricingTier.Name, name);
    }
}
