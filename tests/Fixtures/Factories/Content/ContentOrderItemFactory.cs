using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ContentOrderItemEntity"/> instances in tests.
/// </summary>
public static class ContentOrderItemFactory
{
    /// <summary>Creates an order item for the given order and category.</summary>
    public static ContentOrderItemEntity Create(Guid orderId, Guid categoryId) =>
        new ContentOrderItemBuilder().WithOrderId(orderId).WithCategoryId(categoryId).Build();

    /// <summary>Creates an order item with a promotion level.</summary>
    public static ContentOrderItemEntity CreateWithPromo(
        Guid orderId,
        Guid categoryId,
        Guid promotionLevelId,
        decimal promoPrice
    ) =>
        new ContentOrderItemBuilder()
            .WithOrderId(orderId)
            .WithCategoryId(categoryId)
            .WithPromotionLevelId(promotionLevelId, promoPrice)
            .Build();

    /// <summary>Creates an order item with social boost flag.</summary>
    public static ContentOrderItemEntity CreateSocialBoost(Guid orderId, Guid categoryId) =>
        new ContentOrderItemBuilder().WithOrderId(orderId).WithCategoryId(categoryId).AsSocialBoost().Build();

    /// <summary>Creates a bonus order item.</summary>
    public static ContentOrderItemEntity CreateBonus(Guid orderId, Guid categoryId) =>
        new ContentOrderItemBuilder().WithOrderId(orderId).WithCategoryId(categoryId).AsBonus().Build();

    /// <summary>Creates a list of order items with the specified count.</summary>
    public static List<ContentOrderItemEntity> CreateMany(Guid orderId, Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(orderId, categoryId)).ToList();
}
