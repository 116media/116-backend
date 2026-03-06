using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="CategoryPricingEntity"/> instances in tests.
/// For test code, prefer using CategoryPricingFactory instead of direct Builder usage.
/// </summary>
internal class CategoryPricingBuilder
{
    private Guid _id;
    private Guid _categoryId;
    private Guid _pricingTierId;
    private decimal _priceUsd;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryPricingBuilder"/> class with default values.
    /// </summary>
    public CategoryPricingBuilder(Guid categoryId, Guid pricingTierId)
    {
        _id = Guid.NewGuid();
        _categoryId = categoryId;
        _pricingTierId = pricingTierId;
        _priceUsd = TestConstants.Content.CategoryPricing.ValidPriceUsd;
    }

    /// <summary>Sets the pricing ID.</summary>
    public CategoryPricingBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the price in USD.</summary>
    public CategoryPricingBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>Builds the <see cref="CategoryPricingEntity"/> instance.</summary>
    public CategoryPricingEntity Build()
    {
        return CategoryPricingEntity.Create(_id, _categoryId, _pricingTierId, _priceUsd);
    }
}
