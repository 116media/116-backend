using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="PromotionLevelBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class PromotionLevelFactory
{
    /// <summary>
    /// Creates a promotion level with default random values.
    /// </summary>
    public static PromotionLevelEntity Create() => new PromotionLevelBuilder().Build();

    /// <summary>
    /// Creates a promotion level with specific values.
    /// </summary>
    public static PromotionLevelEntity Create(string name, int durationDays, decimal priceUsd) =>
        new PromotionLevelBuilder().WithName(name).WithDurationDays(durationDays).WithPriceUsd(priceUsd).Build();

    /// <summary>
    /// Creates an inactive promotion level.
    /// </summary>
    public static PromotionLevelEntity CreateInactive() => new PromotionLevelBuilder().AsInactive().Build();

    /// <summary>
    /// Creates a promotion level with known default values.
    /// </summary>
    public static PromotionLevelEntity CreateDefault() =>
        new PromotionLevelBuilder()
            .WithName(TestConstants.PromotionLevel.ValidName)
            .WithDurationDays(TestConstants.PromotionLevel.ValidDurationDays)
            .WithPriceUsd(TestConstants.PromotionLevel.ValidPriceUsd)
            .Build();

    /// <summary>
    /// Creates a list of promotion levels with the specified count.
    /// </summary>
    public static List<PromotionLevelEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
