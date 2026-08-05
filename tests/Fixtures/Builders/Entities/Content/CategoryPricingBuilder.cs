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
        _priceUsd = TestConstants.CategoryPricing.ValidPriceUsd;
    }

    /// <summary>
    /// Sets the pricing ID.
    /// </summary>
    public CategoryPricingBuilder WithId(Guid id)
    {
        _id = id;
        return this;
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
        return CategoryPricingEntity.Create(
            _id,
            _categoryId,
            _pricingTierId,
            _priceUsd,
            TestErrorsFactory.CreateCategoryErrors()
        );
    }
}
