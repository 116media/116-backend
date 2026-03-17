using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ContentItemTierEntity"/> instances in tests.
/// </summary>
public static class ContentItemTierFactory
{
    /// <summary>Creates a tier snapshot for the given order item and pricing tier.</summary>
    public static ContentItemTierEntity Create(Guid orderItemId, Guid pricingTierId, decimal price) =>
        new ContentItemTierBuilder()
            .WithOrderItemId(orderItemId)
            .WithPricingTierId(pricingTierId)
            .WithPriceSnapshotUsd(price)
            .Build();

    /// <summary>Creates a tier snapshot using the default valid price.</summary>
    public static ContentItemTierEntity CreateDefault(Guid orderItemId, Guid pricingTierId) =>
        new ContentItemTierBuilder()
            .WithOrderItemId(orderItemId)
            .WithPricingTierId(pricingTierId)
            .WithPriceSnapshotUsd(TestConstants.Content.Commerce.ValidTierPriceUsd)
            .Build();

    /// <summary>Creates a list of tier snapshots with the specified count.</summary>
    public static List<ContentItemTierEntity> CreateMany(Guid orderItemId, Guid pricingTierId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreateDefault(orderItemId, pricingTierId)).ToList();
}
