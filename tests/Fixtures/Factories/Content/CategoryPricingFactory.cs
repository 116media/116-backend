using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="CategoryPricingBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class CategoryPricingFactory
{
    /// <summary>
    /// Creates a category pricing with default price.
    /// </summary>
    public static CategoryPricingEntity Create(Guid categoryId, Guid pricingTierId) =>
        new CategoryPricingBuilder(categoryId, pricingTierId).Build();

    /// <summary>
    /// Creates a category pricing with a specific price.
    /// </summary>
    public static CategoryPricingEntity Create(Guid categoryId, Guid pricingTierId, decimal priceUsd) =>
        new CategoryPricingBuilder(categoryId, pricingTierId).WithPriceUsd(priceUsd).Build();
}
