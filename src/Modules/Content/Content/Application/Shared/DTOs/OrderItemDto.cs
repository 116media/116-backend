namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a single commissioned content item within an order.
/// </summary>
/// <param name="Id">The unique identifier of the order item.</param>
/// <param name="ContentKind">The kind of content being commissioned (Article or Video).</param>
/// <param name="CategoryName">The display name of the content category.</param>
/// <param name="PromotionLevelName">The name of the promotion level, or null if none selected.</param>
/// <param name="PromoPriceUsd">The snapshotted promotion level price in USD, or null if none selected.</param>
/// <param name="SocialBoost">Whether social media promotion was requested for this item.</param>
/// <param name="IsBonus">Whether this is a bonus/complimentary item.</param>
/// <param name="Tiers">The pricing tier snapshots attached to this item.</param>
public record OrderItemDto(
    Guid Id,
    string ContentKind,
    string CategoryName,
    string? PromotionLevelName,
    decimal? PromoPriceUsd,
    bool SocialBoost,
    bool IsBonus,
    IReadOnlyList<ItemTierDto> Tiers
);
