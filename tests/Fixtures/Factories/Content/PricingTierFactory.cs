using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="PricingTierEntity"/> instances in tests.
/// </summary>
public static class PricingTierFactory
{
    /// <summary>Creates a pricing tier with default random values.</summary>
    public static PricingTierEntity Create() => new PricingTierBuilder().Build();

    /// <summary>Creates a pricing tier with a specific name.</summary>
    public static PricingTierEntity Create(string name) => new PricingTierBuilder().WithName(name).Build();

    /// <summary>Creates a pricing tier with a specific ID.</summary>
    public static PricingTierEntity CreateWithId(Guid id) => new PricingTierBuilder().WithId(id).Build();

    /// <summary>Creates an inactive pricing tier.</summary>
    public static PricingTierEntity CreateInactive() => new PricingTierBuilder().AsInactive().Build();

    /// <summary>Creates a pricing tier with a name and description.</summary>
    public static PricingTierEntity CreateWithDescription(string name, string description) =>
        new PricingTierBuilder().WithName(name).WithDescription(description).Build();

    /// <summary>Creates a pricing tier with a known default name (ValidName).</summary>
    public static PricingTierEntity CreateDefault() =>
        new PricingTierBuilder().WithName(TestConstants.Content.PricingTier.ValidName).Build();

    /// <summary>Creates a list of pricing tiers with the specified count.</summary>
    public static List<PricingTierEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
