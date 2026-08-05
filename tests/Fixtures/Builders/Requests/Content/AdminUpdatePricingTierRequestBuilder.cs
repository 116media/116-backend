using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdatePricingTierRequest"/> instances in tests.
/// </summary>
public class AdminUpdatePricingTierRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;
    private string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdatePricingTierRequestBuilder"/> class
    /// with valid random default values that satisfy the update pricing tier validator.
    /// </summary>
    public AdminUpdatePricingTierRequestBuilder()
    {
        string candidate = $"tier_{_faker.Random.AlphaNumeric(length: 8)}";
        _name = candidate[..Math.Min(TestConstants.PricingTier.NameMaxLength, candidate.Length)];
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the pricing tier name.
    /// </summary>
    /// <param name="name">The pricing tier name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdatePricingTierRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the pricing tier description.
    /// </summary>
    /// <param name="description">The pricing tier description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdatePricingTierRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdatePricingTierRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdatePricingTierRequest instance.</returns>
    public AdminUpdatePricingTierRequest Build()
    {
        return new AdminUpdatePricingTierRequest(Name: _name, Description: _description);
    }
}
