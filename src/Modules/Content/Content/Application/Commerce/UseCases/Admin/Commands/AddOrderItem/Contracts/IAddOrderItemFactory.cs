using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;

/// <summary>
/// Factory for adding a commissioned content item to a draft order.
/// Validates category, optionally resolves promotion level, creates and persists the item.
/// </summary>
public interface IAddOrderItemFactory
{
    /// <summary>
    /// Validates the order is Draft, validates the category is commissionable,
    /// optionally snapshots the promotion price, creates and persists the item.
    /// Returns the created item entity together with the resolved names needed for the response DTO.
    /// </summary>
    Task<(ContentOrderItemEntity Item, string CategoryName, string? PromotionLevelName)> CreateItemAsync(
        ContentOrderEntity order,
        EnumCoreContentType contentKind,
        Guid categoryId,
        Guid? promotionLevelId,
        bool socialBoost,
        bool isBonus,
        CancellationToken ct
    );
}
