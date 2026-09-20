using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a single pricing tier snapshot attached to a <see cref="ContentOrderItemEntity" />.
/// The price is frozen from <c>category_pricing.price_usd</c> at the moment the tier is added,
/// guaranteeing the client's quote cannot change retroactively even if the admin adjusts
/// category pricing later.
/// </summary>
public class ContentItemTierEntity : Entity<Guid>
{
    /// <summary>
    /// The order item this tier belongs to.
    /// </summary>
    public Guid OrderItemId { get; private set; }

    /// <summary>
    /// The pricing tier being snapshotted (e.g., "base_upload", "social_boost").
    /// </summary>
    public Guid PricingTierId { get; private set; }

    /// <summary>
    /// The price frozen from <c>category_pricing.price_usd</c> at the moment this tier was added.
    /// Immutable after creation.
    /// </summary>
    public Money PriceSnapshotUsd { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ContentItemTierEntity() { }

    /// <summary>
    /// Creates a new immutable pricing tier snapshot for an order item.
    /// </summary>
    /// <param name="id">The unique identifier for this tier snapshot.</param>
    /// <param name="orderItemId">The parent order item identifier.</param>
    /// <param name="pricingTierId">The pricing tier being snapshotted.</param>
    /// <param name="priceSnapshotUsd">The price frozen from category pricing at this moment.</param>
    /// <returns>A new <see cref="ContentItemTierEntity" /> instance.</returns>
    internal static ContentItemTierEntity Create(
        Guid id,
        Guid orderItemId,
        Guid pricingTierId,
        decimal priceSnapshotUsd
    )
    {
        return new ContentItemTierEntity
        {
            Id = id,
            OrderItemId = orderItemId,
            PricingTierId = pricingTierId,
            PriceSnapshotUsd = priceSnapshotUsd,
        };
    }
}
