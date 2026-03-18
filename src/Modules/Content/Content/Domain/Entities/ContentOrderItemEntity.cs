using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a single commissioned content item within a <see cref="ContentOrderEntity" />.
/// Each item specifies what category the content belongs to, what kind of content it is
/// (Article or Video), and optional promotion options (social boost, featured placement).
/// After payment is verified, the admin creates the actual article or video and links it
/// back to this order item via <c>order_item_id</c>.
/// </summary>
public class ContentOrderItemEntity : Aggregate<Guid>
{
    /// <summary>
    /// The order this item belongs to.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// The kind of content being commissioned (Article or Video).
    /// Short videos are not sold as commissioned items.
    /// </summary>
    public EnumCoreContentType ContentKind { get; private set; }

    /// <summary>
    /// The category the commissioned content belongs to (e.g., "Artist Profile", "Le Focus").
    /// Must be an active, paid (IsFree = false) category.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// The optional homepage promotion level applied to this item.
    /// When set, the linked article or video will be stamped as featured after payment.
    /// </summary>
    public Guid? PromotionLevelId { get; private set; }

    /// <summary>
    /// The promotion level price snapshotted at item creation time.
    /// Immutable after creation — changes to the promotion level price do not affect this item.
    /// <c>null</c> when no promotion level is selected.
    /// </summary>
    public decimal? PromoPriceSnapshotUsd { get; private set; }

    /// <summary>
    /// Whether the customer has requested a social media promotion (Facebook/Instagram) for this item.
    /// Stamped on the linked article or video after payment is verified.
    /// </summary>
    public bool SocialBoost { get; private set; }

    /// <summary>
    /// Whether this item is a bonus entry (e.g., a complimentary slot from a package deal).
    /// Bonus items do not contribute to the order total.
    /// </summary>
    public bool IsBonus { get; private set; }

    /// <summary>
    /// The order this item belongs to.
    /// </summary>
    public ContentOrderEntity Order { get; private set; } = null!;

    /// <summary>
    /// The category for this commissioned content item.
    /// </summary>
    public CategoryEntity Category { get; private set; } = null!;

    /// <summary>
    /// The promotion level applied to this item, or <c>null</c> if none was selected.
    /// </summary>
    public PromotionLevelEntity? PromotionLevel { get; private set; }

    /// <summary>
    /// The pricing tier snapshots attached to this item.
    /// </summary>
    public ICollection<ContentItemTierEntity> Tiers { get; } = new List<ContentItemTierEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ContentOrderItemEntity() { }

    /// <summary>
    /// Creates a new commissioned content order item.
    /// </summary>
    /// <param name="id">The unique identifier for the item.</param>
    /// <param name="orderId">The parent order identifier.</param>
    /// <param name="contentKind">The kind of content being commissioned.</param>
    /// <param name="categoryId">The category for this content item.</param>
    /// <param name="promotionLevelId">The optional promotion level identifier.</param>
    /// <param name="promoPriceSnapshotUsd">The snapshotted promotion level price, or <c>null</c>.</param>
    /// <param name="socialBoost">Whether social media promotion is requested.</param>
    /// <param name="isBonus">Whether this is a bonus/complimentary item.</param>
    /// <returns>A new <see cref="ContentOrderItemEntity" /> instance.</returns>
    public static ContentOrderItemEntity Create(
        Guid id,
        Guid orderId,
        EnumCoreContentType contentKind,
        Guid categoryId,
        Guid? promotionLevelId,
        decimal? promoPriceSnapshotUsd,
        bool socialBoost,
        bool isBonus
    )
    {
        return new ContentOrderItemEntity
        {
            Id = id,
            OrderId = orderId,
            ContentKind = contentKind,
            CategoryId = categoryId,
            PromotionLevelId = promotionLevelId,
            PromoPriceSnapshotUsd = promoPriceSnapshotUsd,
            SocialBoost = socialBoost,
            IsBonus = isBonus,
        };
    }
}
