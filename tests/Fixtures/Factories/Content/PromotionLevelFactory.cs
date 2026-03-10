using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="PromotionLevelEntity"/> instances in tests.
/// </summary>
public static class PromotionLevelFactory
{
    /// <summary>Creates a promotion level with default random values.</summary>
    public static PromotionLevelEntity Create() => new PromotionLevelBuilder().Build();

    /// <summary>Creates a promotion level with specific values.</summary>
    public static PromotionLevelEntity Create(string name, int durationDays, decimal priceUsd) =>
        new PromotionLevelBuilder().WithName(name).WithDurationDays(durationDays).WithPriceUsd(priceUsd).Build();

    /// <summary>Creates a promotion level with a specific ID.</summary>
    public static PromotionLevelEntity CreateWithId(Guid id) => new PromotionLevelBuilder().WithId(id).Build();

    /// <summary>Creates an inactive promotion level.</summary>
    public static PromotionLevelEntity CreateInactive() => new PromotionLevelBuilder().AsInactive().Build();

    /// <summary>Creates a promotion level with known default values.</summary>
    public static PromotionLevelEntity CreateDefault() =>
        new PromotionLevelBuilder()
            .WithName(TestConstants.Content.PromotionLevel.ValidName)
            .WithDurationDays(TestConstants.Content.PromotionLevel.ValidDurationDays)
            .WithPriceUsd(TestConstants.Content.PromotionLevel.ValidPriceUsd)
            .Build();

    /// <summary>Creates a list of promotion levels with the specified count.</summary>
    public static List<PromotionLevelEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
