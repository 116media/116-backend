using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PromotionLevelEntity"/> instances in tests.
/// For test code, prefer using PromotionLevelFactory instead of direct Builder usage.
/// </summary>
internal class PromotionLevelBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _name;
    private int _durationDays;
    private decimal _priceUsd;
    private bool _isActive = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotionLevelBuilder"/> class with random default values.
    /// </summary>
    public PromotionLevelBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        _name = word[..Math.Min(TestConstants.Content.PromotionLevel.NameMaxLength, word.Length)];
        _durationDays = _faker.Random.Int(1, 30);
        _priceUsd = _faker.Random.Decimal(0, 500);
    }

    /// <summary>Sets the promotion level ID.</summary>
    public PromotionLevelBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the promotion level name.</summary>
    public PromotionLevelBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>Sets the duration in days.</summary>
    public PromotionLevelBuilder WithDurationDays(int durationDays)
    {
        _durationDays = durationDays;
        return this;
    }

    /// <summary>Sets the price in USD.</summary>
    public PromotionLevelBuilder WithPriceUsd(decimal priceUsd)
    {
        _priceUsd = priceUsd;
        return this;
    }

    /// <summary>Marks the promotion level as inactive.</summary>
    public PromotionLevelBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>Builds the <see cref="PromotionLevelEntity"/> instance.</summary>
    public PromotionLevelEntity Build()
    {
        var entity = PromotionLevelEntity.Create(_id, _name, _durationDays, _priceUsd);

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
