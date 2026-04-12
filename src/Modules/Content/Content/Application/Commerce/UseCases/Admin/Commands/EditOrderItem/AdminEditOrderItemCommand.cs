using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Command for editing a content item in a draft order.
/// </summary>
/// <param name="OrderId">The identifier of the order containing the item.</param>
/// <param name="ItemId">The identifier of the item to edit.</param>
/// <param name="ContentKind">The new content kind, or null to keep the current one.</param>
/// <param name="CategoryId">The new category ID, or null to keep the current one.</param>
/// <param name="PromotionLevelId">The new promotion level ID, or null to clear it.</param>
/// <param name="SocialBoost">The new social boost flag, or null to keep the current one.</param>
/// <param name="IsBonus">The new bonus flag, or null to keep the current one.</param>
public record AdminEditOrderItemCommand(
    string OrderId,
    string ItemId,
    EnumCoreContentType? ContentKind,
    string? CategoryId,
    Guid? PromotionLevelId,
    bool? SocialBoost,
    bool? IsBonus
) : ICommand<AdminEditOrderItemResult>;

/// <summary>
/// Result of the <see cref="AdminEditOrderItemCommand" /> containing the updated order item.
/// </summary>
/// <param name="Item">The updated order item.</param>
public record AdminEditOrderItemResult(OrderItemDto Item);
