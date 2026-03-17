using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentOrderItemEntity"/> instances in tests.
/// For test code, prefer using ContentOrderItemFactory instead of direct Builder usage.
/// </summary>
internal class ContentOrderItemBuilder
{
    private Guid _id;
    private Guid _orderId;
    private EnumCoreContentType _contentKind = EnumCoreContentType.Article;
    private Guid _categoryId;
    private Guid? _promotionLevelId;
    private decimal? _promoPriceSnapshotUsd;
    private bool _socialBoost;
    private bool _isBonus;

    public ContentOrderItemBuilder()
    {
        _id = Guid.NewGuid();
        _orderId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
    }

    public ContentOrderItemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ContentOrderItemBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
    }

    public ContentOrderItemBuilder WithContentKind(EnumCoreContentType contentKind)
    {
        _contentKind = contentKind;
        return this;
    }

    public ContentOrderItemBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ContentOrderItemBuilder WithPromotionLevelId(Guid promotionLevelId, decimal priceSnapshot)
    {
        _promotionLevelId = promotionLevelId;
        _promoPriceSnapshotUsd = priceSnapshot;
        return this;
    }

    public ContentOrderItemBuilder AsSocialBoost()
    {
        _socialBoost = true;
        return this;
    }

    public ContentOrderItemBuilder AsBonus()
    {
        _isBonus = true;
        return this;
    }

    public ContentOrderItemEntity Build()
    {
        return ContentOrderItemEntity.Create(
            _id,
            _orderId,
            _contentKind,
            _categoryId,
            _promotionLevelId,
            _promoPriceSnapshotUsd,
            _socialBoost,
            _isBonus
        );
    }
}
