using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminAddCategoryPricingRequest"/> instances in tests.
/// Defaults produce values that satisfy <c>AdminAddCategoryPricingValidator</c>.
/// </summary>
public class AdminAddCategoryPricingRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _pricingTierId;
    private decimal _priceUsd;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAddCategoryPricingRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminAddCategoryPricingRequestBuilder()
    {
        _pricingTierId = _faker.Random.Guid();
        _priceUsd = _faker.Random.Decimal(min: 1m, max: 999m);
    }

    /// <summary>
    /// Sets the identifier of the pricing tier to attach to the category.
    /// </summary>
    /// <param name="pricingTierId">The pricing tier identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddCategoryPricingRequestBuilder WithPricingTierId(Guid pricingTierId)
    {
        _pricingTierId = pricingTierId;
        return this;
    }

    /// <summary>
    /// Sets the price in USD for this tier within the category.
    /// </summary>
    /// <param name="priceUsd">The price in USD.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddCategoryPricingRequestBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminAddCategoryPricingRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminAddCategoryPricingRequest instance.</returns>
    public AdminAddCategoryPricingRequest Build()
    {
        return new AdminAddCategoryPricingRequest(PricingTierId: _pricingTierId, PriceUsd: _priceUsd);
    }
}
