using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Catalog.Specifications;

/// <summary>
/// Specification that matches all pricing rows for a given category.
/// </summary>
public class CategoryPricingByCategorySpecification(Guid categoryId) : Specification<CategoryPricingEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryPricingEntity, bool>> ToExpression()
    {
        return pricing => pricing.CategoryId == categoryId;
    }
}

/// <summary>
/// Specification that matches a specific pricing row by category and pricing tier identifiers.
/// </summary>
public class CategoryPricingByIdsSpecification(Guid categoryId, Guid pricingTierId)
    : Specification<CategoryPricingEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryPricingEntity, bool>> ToExpression()
    {
        return pricing => pricing.CategoryId == categoryId && pricing.PricingTierId == pricingTierId;
    }
}
