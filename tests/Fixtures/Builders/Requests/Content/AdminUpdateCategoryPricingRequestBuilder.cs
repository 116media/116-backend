using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateCategoryPricingRequest"/> instances in tests.
/// Defaults produce values that satisfy <c>AdminUpdateCategoryPricingValidator</c>.
/// </summary>
public class AdminUpdateCategoryPricingRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private decimal _priceUsd;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateCategoryPricingRequestBuilder"/> class
    /// with a valid random value that satisfies the validator.
    /// </summary>
    public AdminUpdateCategoryPricingRequestBuilder()
    {
        _priceUsd = _faker.Random.Decimal(min: 1m, max: 999m);
    }

    /// <summary>
    /// Sets the new price in USD for the category pricing tier.
    /// </summary>
    /// <param name="priceUsd">The new price in USD.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateCategoryPricingRequestBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateCategoryPricingRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateCategoryPricingRequest instance.</returns>
    public AdminUpdateCategoryPricingRequest Build()
    {
        return new AdminUpdateCategoryPricingRequest(PriceUsd: _priceUsd);
    }
}
