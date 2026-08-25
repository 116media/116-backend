using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PromotionLevelEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; PromotionLevelFactory only names chains three or more tests share.
/// </summary>
public class PromotionLevelBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private string _name;
    private int _durationDays;
    private decimal _priceUsd;
    private bool _isActive = true;
    private int _spotPriority = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotionLevelBuilder"/> class with random default values.
    /// </summary>
    public PromotionLevelBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        string prefix = word.Length > 4 ? word[..4] : word;
        string unique = $"{prefix}{Guid.NewGuid():N}";
        _name = unique[..Math.Min(TestConstants.PromotionLevel.NameMaxLength, unique.Length)];
        _durationDays = _faker.Random.Int(1, 30);
        _priceUsd = _faker.Random.Decimal(0, 500);
        _spotPriority = _faker.Random.Int(1, 3);
    }

    /// <summary>
    /// Sets the promotion level name.
    /// </summary>
    public PromotionLevelBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the duration in days.
    /// </summary>
    public PromotionLevelBuilder WithDurationDays(int durationDays)
    {
        _durationDays = durationDays;
        return this;
    }

    /// <summary>
    /// Sets the price in USD.
    /// </summary>
    public PromotionLevelBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>
    /// Marks the promotion level as inactive.
    /// </summary>
    public PromotionLevelBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Sets the spot priority (1, 2, or 3).
    /// </summary>
    public PromotionLevelBuilder WithSpotPriority(int spotPriority)
    {
        _spotPriority = spotPriority;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PromotionLevelEntity"/> instance.
    /// </summary>
    public PromotionLevelEntity Build()
    {
        var entity = PromotionLevelEntity.Create(
            _id,
            _name,
            _durationDays,
            _priceUsd,
            _spotPriority,
            TestErrorsFactory.CreatePromotionLevelErrors()
        );

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
