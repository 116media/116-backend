using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentItemTierEntity"/> instances in tests.
/// For test code, prefer using ContentItemTierFactory instead of direct Builder usage.
/// </summary>
internal class ContentItemTierBuilder
{
    private Guid _id;
    private Guid _orderItemId;
    private Guid _pricingTierId;
    private decimal _priceSnapshotUsd = TestConstants.Content.Commerce.ValidTierPriceUsd;

    public ContentItemTierBuilder()
    {
        _id = Guid.NewGuid();
        _orderItemId = Guid.NewGuid();
        _pricingTierId = Guid.NewGuid();
    }

    public ContentItemTierBuilder WithId(Guid id)
    {
        _id = id;
        return this;
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
