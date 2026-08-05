using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreatePromotionLevelRequest"/> instances in tests.
/// </summary>
public class AdminCreatePromotionLevelRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;
    private int _durationDays;
    private decimal _priceUsd;
    private int? _spotPriority;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreatePromotionLevelRequestBuilder"/> class
    /// with valid random default values that satisfy the create promotion level validator.
    /// </summary>
    public AdminCreatePromotionLevelRequestBuilder()
    {
        string candidate = $"Level {_faker.Random.AlphaNumeric(length: 8)}";
        _name = candidate[..Math.Min(TestConstants.PromotionLevel.NameMaxLength, candidate.Length)];
        _durationDays = TestConstants.PromotionLevel.ValidDurationDays;
        _priceUsd = TestConstants.PromotionLevel.ValidPriceUsd;
        _spotPriority = null;
    }

    /// <summary>
    /// Sets the promotion level name.
    /// </summary>
    /// <param name="name">The promotion level name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePromotionLevelRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the homepage placement duration in days.
    /// </summary>
    /// <param name="durationDays">The duration in days.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePromotionLevelRequestBuilder WithDurationDays(int durationDays)
    {
        _durationDays = durationDays;
        return this;
    }

    /// <summary>
    /// Sets the promotion level price in USD.
    /// </summary>
    /// <param name="priceUsd">The price in US dollars.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePromotionLevelRequestBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>
    /// Sets the homepage grid spot priority (1, 2, or 3), or null for no dedicated spot.
    /// </summary>
    /// <param name="spotPriority">The spot priority.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePromotionLevelRequestBuilder WithSpotPriority(int? spotPriority)
    {
        _spotPriority = spotPriority;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreatePromotionLevelRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreatePromotionLevelRequest instance.</returns>
    public AdminCreatePromotionLevelRequest Build()
    {
        return new AdminCreatePromotionLevelRequest(
            Name: _name,
            DurationDays: _durationDays,
            PriceUsd: _priceUsd,
            SpotPriority: _spotPriority
        );
    }
}
