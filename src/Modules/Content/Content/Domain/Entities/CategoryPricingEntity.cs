using _116.Content.Application.Shared.Errors;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a pricing tier assignment for a category, recording the price for a specific
/// add-on service (e.g., "Artist Profile + base_upload = USD25").
/// </summary>
public class CategoryPricingEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identifier of the category this pricing row belongs to.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// The identifier of the pricing tier being configured.
    /// </summary>
    public Guid PricingTierId { get; private set; }

    /// <summary>
    /// The price in USD for this tier within this category.
    /// </summary>
    public decimal PriceUsd { get; private set; }

    /// <summary>
    /// The category this pricing row belongs to.
    /// </summary>
    public CategoryEntity Category { get; private set; } = null!;

    /// <summary>
    /// The pricing tier being configured.
    /// </summary>
    public PricingTierEntity PricingTier { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private CategoryPricingEntity() { }

    /// <summary>
    /// Creates a new category pricing entity.
    /// </summary>
    /// <param name="id">The unique identifier for the pricing row.</param>
    /// <param name="categoryId">The identifier of the category.</param>
    /// <param name="pricingTierId">The identifier of the pricing tier.</param>
    /// <param name="priceUsd">The price in USD (must be >= 0).</param>
    /// <returns>A new <see cref="CategoryPricingEntity" /> instance.</returns>
    public static CategoryPricingEntity Create(Guid id, Guid categoryId, Guid pricingTierId, decimal priceUsd)
    {
        if (priceUsd < 0)
        {
            throw CategoryErrors.PriceMustBeNonNegative();
        }

        return new CategoryPricingEntity
        {
            Id = id,
            CategoryId = categoryId,
            PricingTierId = pricingTierId,
            PriceUsd = priceUsd,
        };
    }

    /// <summary>
    /// Updates the price for this pricing tier within the category.
    /// </summary>
    /// <param name="priceUsd">The new price in USD (must be >= 0).</param>
    public void UpdatePrice(decimal priceUsd)
    {
        if (priceUsd < 0)
        {
            throw CategoryErrors.PriceMustBeNonNegative();
        }

        PriceUsd = priceUsd;
    }
}
