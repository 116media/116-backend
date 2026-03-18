using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Command for adding a commissioned content item to a draft order.
/// </summary>
/// <param name="OrderId">The identifier of the order to add the item to.</param>
/// <param name="ContentKind">The kind of content being commissioned (Article or Video).</param>
/// <param name="CategoryId">The category for this content item (must be active and paid).</param>
/// <param name="PromotionLevelId">An optional homepage promotion level for this item.</param>
/// <param name="SocialBoost">Whether social media promotion is requested for this item.</param>
/// <param name="IsBonus">Whether this is a bonus/complimentary item.</param>
public record AdminAddOrderItemCommand(
    string OrderId,
    EnumCoreContentType ContentKind,
    string CategoryId,
    Guid? PromotionLevelId,
    bool SocialBoost,
    bool IsBonus
) : ICommand<AdminAddOrderItemResult>;

/// <summary>
/// Result of the <see cref="AdminAddOrderItemCommand" /> containing the created order item.
/// </summary>
/// <param name="Item">The created order item.</param>
public record AdminAddOrderItemResult(OrderItemDto Item);
