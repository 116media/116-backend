using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentOrderItemEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; ContentOrderItemFactory only names chains three or more tests share.
/// </summary>
public class ContentOrderItemBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _orderId = Guid.NewGuid();
    private EnumCoreContentType _contentKind = EnumCoreContentType.Article;
    private Guid _categoryId = Guid.NewGuid();
    private Guid? _promotionLevelId;
    private decimal? _promoPriceSnapshotUsd;
    private bool _socialBoost;
    private bool _isBonus;
    private CategoryEntity? _category;

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

    /// <summary>
    /// Populates the category navigation, as loading the item with its category
    /// included would.
    /// </summary>
    public ContentOrderItemBuilder WithCategory(CategoryEntity category)
    {
        _category = category;
        _categoryId = category.Id;
        return this;
    }

    public ContentOrderItemEntity Build()
    {
        ContentOrderItemEntity entity = ContentOrderItemEntity.Create(
            _id,
            _orderId,
            _contentKind,
            _categoryId,
            _promotionLevelId,
            _promoPriceSnapshotUsd,
            _socialBoost,
            _isBonus
        );

        if (_category is not null)
        {
            PropertyInfo prop = typeof(ContentOrderItemEntity).GetProperty(
                nameof(ContentOrderItemEntity.Category),
                BindingFlags.Public | BindingFlags.Instance
            )!;

            prop.SetValue(entity, _category);
        }

        return entity;
    }
}
