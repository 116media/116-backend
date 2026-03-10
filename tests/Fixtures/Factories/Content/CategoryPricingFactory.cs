using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="CategoryPricingEntity"/> instances in tests.
/// </summary>
public static class CategoryPricingFactory
{
    /// <summary>Creates a category pricing with default price.</summary>
    public static CategoryPricingEntity Create(Guid categoryId, Guid pricingTierId) =>
        new CategoryPricingBuilder(categoryId, pricingTierId).Build();

    /// <summary>Creates a category pricing with a specific price.</summary>
    public static CategoryPricingEntity Create(Guid categoryId, Guid pricingTierId, decimal priceUsd) =>
        new CategoryPricingBuilder(categoryId, pricingTierId).WithPriceUsd(priceUsd).Build();

    /// <summary>Creates a category pricing with zero price.</summary>
    public static CategoryPricingEntity CreateFree(Guid categoryId, Guid pricingTierId) =>
        new CategoryPricingBuilder(categoryId, pricingTierId)
            .WithPriceUsd(TestConstants.Content.CategoryPricing.ZeroPriceUsd)
            .Build();
}
