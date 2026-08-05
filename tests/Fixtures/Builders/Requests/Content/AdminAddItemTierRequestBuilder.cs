using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminAddItemTierRequest"/> instances in tests.
/// </summary>
public class AdminAddItemTierRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _pricingTierId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAddItemTierRequestBuilder"/> class
    /// with a valid random pricing tier identifier that satisfies the validator.
    /// </summary>
    public AdminAddItemTierRequestBuilder()
    {
        _pricingTierId = _faker.Random.Guid().ToString();
    }

    /// <summary>
    /// Sets the identifier of the pricing tier to snapshot onto the order item.
    /// </summary>
    /// <param name="pricingTierId">The pricing tier identifier as a string GUID.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddItemTierRequestBuilder WithPricingTierId(string pricingTierId)
    {
        _pricingTierId = pricingTierId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminAddItemTierRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminAddItemTierRequest instance.</returns>
    public AdminAddItemTierRequest Build()
    {
        return new AdminAddItemTierRequest(PricingTierId: _pricingTierId);
    }
}
