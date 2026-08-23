using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreatePricingTierRequest"/> instances in tests.
/// </summary>
public class AdminCreatePricingTierRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;
    private string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreatePricingTierRequestBuilder"/> class
    /// with valid random default values that satisfy the create pricing tier validator.
    /// </summary>
    public AdminCreatePricingTierRequestBuilder()
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
    public AdminCreatePricingTierRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the pricing tier description.
    /// </summary>
    /// <param name="description">The pricing tier description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePricingTierRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreatePricingTierRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreatePricingTierRequest instance.</returns>
    public AdminCreatePricingTierRequest Build()
    {
        return new AdminCreatePricingTierRequest(Name: _name, Description: _description);
    }
}
