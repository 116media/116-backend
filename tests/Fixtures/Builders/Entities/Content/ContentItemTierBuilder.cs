using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentItemTierEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; ContentItemTierFactory only names chains three or more tests share.
/// </summary>
public class ContentItemTierBuilder
{
    private Guid _id;
    private Guid _orderItemId;
    private Guid _pricingTierId;
    private decimal _priceSnapshotUsd = TestConstants.Commerce.ValidTierPriceUsd;

    public ContentItemTierBuilder()
    {
        _id = Guid.NewGuid();
        _orderItemId = Guid.NewGuid();
        _pricingTierId = Guid.NewGuid();
    }

    public ContentItemTierBuilder WithOrderItemId(Guid orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    public ContentItemTierBuilder WithPricingTierId(Guid pricingTierId)
    {
        _pricingTierId = pricingTierId;
        return this;
    }

    public ContentItemTierBuilder WithPriceSnapshotUsd(decimal priceSnapshotUsd)
    {
        _priceSnapshotUsd = priceSnapshotUsd;
        return this;
    }

    public ContentItemTierEntity Build()
    {
        return ContentItemTierEntity.Create(_id, _orderItemId, _pricingTierId, _priceSnapshotUsd);
    }
}
