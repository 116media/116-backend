using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="CategoryPricingEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; CategoryPricingFactory only names chains three or more tests share.
/// </summary>
public class CategoryPricingBuilder
{
    private readonly CategoryEntity _category;
    private readonly Guid _pricingTierId;
    private decimal _priceUsd;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryPricingBuilder"/> class with default values.
    /// </summary>
    public CategoryPricingBuilder(CategoryEntity category, Guid pricingTierId)
    {
        _category = category;
        _pricingTierId = pricingTierId;
        _priceUsd = TestConstants.CategoryPricing.ValidPriceUsd;
    }

    /// <summary>
    /// Sets the price in USD.
    /// </summary>
    public CategoryPricingBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="CategoryPricingEntity"/> instance.
    /// </summary>
    public CategoryPricingEntity Build()
    {
        _category.SetPricing(pricingTierId: _pricingTierId, priceUsd: _priceUsd);

        return _category.FindPricing(pricingTierId: _pricingTierId)!;
    }
}
