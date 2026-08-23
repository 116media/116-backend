using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ContentItemTierBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ContentItemTierFactory
{
    /// <summary>
    /// Creates a tier snapshot for the given order item and pricing tier.
    /// </summary>
    public static ContentItemTierEntity Create(Guid orderItemId, Guid pricingTierId, decimal price) =>
        new ContentItemTierBuilder()
            .WithOrderItemId(orderItemId)
            .WithPricingTierId(pricingTierId)
            .WithPriceSnapshotUsd(price)
            .Build();

    /// <summary>
    /// Creates a tier snapshot using the default valid price.
    /// </summary>
    public static ContentItemTierEntity CreateDefault(Guid orderItemId, Guid pricingTierId) =>
        new ContentItemTierBuilder()
            .WithOrderItemId(orderItemId)
            .WithPricingTierId(pricingTierId)
            .WithPriceSnapshotUsd(TestConstants.Commerce.ValidTierPriceUsd)
            .Build();
}
