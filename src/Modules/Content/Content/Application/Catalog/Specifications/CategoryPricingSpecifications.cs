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
/// Specification that matches every pricing row belonging to any of the given categories.
/// </summary>
public class CategoryPricingByCategoriesSpecification(IReadOnlyCollection<Guid> categoryIds)
    : Specification<CategoryPricingEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryPricingEntity, bool>> ToExpression()
    {
        return pricing => categoryIds.Contains(pricing.CategoryId);
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
