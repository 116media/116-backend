using _116.Content.Domain.Entities;
using _116.Content.TestData.Builders.Entities;

namespace _116.Content.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="ContentOrderItemBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ContentOrderItemFactory
{
    /// <summary>
    /// Creates an order item for the given order and category.
    /// </summary>
    public static ContentOrderItemEntity Create(Guid orderId, Guid categoryId) =>
        new ContentOrderItemBuilder().WithOrderId(orderId).WithCategoryId(categoryId).Build();

    /// <summary>
    /// Creates an order item with a promotion level.
    /// </summary>
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

    /// <summary>
    /// Creates an order item with social boost flag.
    /// </summary>
    public static ContentOrderItemEntity CreateSocialBoost(Guid orderId, Guid categoryId) =>
        new ContentOrderItemBuilder().WithOrderId(orderId).WithCategoryId(categoryId).AsSocialBoost().Build();

    /// <summary>
    /// Creates a bonus order item.
    /// </summary>
    public static ContentOrderItemEntity CreateBonus(Guid orderId, Guid categoryId) =>
        new ContentOrderItemBuilder().WithOrderId(orderId).WithCategoryId(categoryId).AsBonus().Build();
}
