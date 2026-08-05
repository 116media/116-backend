using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdatePromotionLevelRequest"/> instances in tests.
/// </summary>
public class AdminUpdatePromotionLevelRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;
    private int _durationDays;
    private decimal _priceUsd;
    private int? _spotPriority;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdatePromotionLevelRequestBuilder"/> class
    /// with valid random default values that satisfy the update promotion level validator.
    /// </summary>
    public AdminUpdatePromotionLevelRequestBuilder()
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
    public AdminUpdatePromotionLevelRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the homepage placement duration in days.
    /// </summary>
    /// <param name="durationDays">The duration in days.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdatePromotionLevelRequestBuilder WithDurationDays(int durationDays)
    {
        _durationDays = durationDays;
        return this;
    }

    /// <summary>
    /// Sets the promotion level price in USD.
    /// </summary>
    /// <param name="priceUsd">The price in US dollars.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdatePromotionLevelRequestBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>
    /// Sets the homepage grid spot priority (1, 2, or 3), or null for no dedicated spot.
    /// </summary>
    /// <param name="spotPriority">The spot priority.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdatePromotionLevelRequestBuilder WithSpotPriority(int? spotPriority)
    {
        _spotPriority = spotPriority;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdatePromotionLevelRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdatePromotionLevelRequest instance.</returns>
    public AdminUpdatePromotionLevelRequest Build()
    {
        return new AdminUpdatePromotionLevelRequest(
            Name: _name,
            DurationDays: _durationDays,
            PriceUsd: _priceUsd,
            SpotPriority: _spotPriority
        );
    }
}
