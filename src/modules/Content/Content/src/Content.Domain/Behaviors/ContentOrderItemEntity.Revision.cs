using _116.Content.Domain.Enums;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="ContentOrderItemEntity" />. Its state lives in <c>Entities/ContentOrderItemEntity.cs</c>.
/// </summary>
public partial class ContentOrderItemEntity
{
    /// <summary>
    /// Updates the order item's properties. Only non-null parameters are applied.
    /// Call this only when the parent order is in <c>Draft</c> status.
    /// </summary>
    /// <param name="contentKind">The new content kind, or null to keep the current one.</param>
    /// <param name="categoryId">The new category ID, or null to keep the current one.</param>
    /// <param name="promotionLevelId">The new promotion level ID, or null to clear it.</param>
    /// <param name="promoPriceSnapshotUsd">The new promotion price snapshot, or null to clear it.</param>
    /// <param name="socialBoost">The new social boost flag, or null to keep the current one.</param>
    /// <param name="isBonus">The new bonus flag, or null to keep the current one.</param>
    public void Update(
        EnumCoreContentType? contentKind,
        Guid? categoryId,
        Guid? promotionLevelId,
        decimal? promoPriceSnapshotUsd,
        bool? socialBoost,
        bool? isBonus
    )
    {
        if (contentKind.HasValue)
        {
            ContentKind = contentKind.Value;
        }

        if (categoryId.HasValue)
        {
            CategoryId = categoryId.Value;
        }

        PromotionLevelId = promotionLevelId;
        PromoPriceSnapshotUsd = promoPriceSnapshotUsd;

        if (socialBoost.HasValue)
        {
            SocialBoost = socialBoost.Value;
        }

        if (isBonus.HasValue)
        {
            IsBonus = isBonus.Value;
        }
    }
}
