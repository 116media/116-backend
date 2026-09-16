using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a pricing tier assignment for a category, recording the price for a specific
/// add-on service (e.g., "Artist Profile + base_upload = USD25").
/// </summary>
public class CategoryPricingEntity : Entity<Guid>
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
    public Money PriceUsd { get; private set; } = null!;

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
    internal static CategoryPricingEntity Create(Guid id, Guid categoryId, Guid pricingTierId, decimal priceUsd)
    {
        if (priceUsd < 0)
        {
            throw new ContentRuleException(ContentRuleCodes.CategoryPriceMustBeNonNegative);
        }

        var pricing = new CategoryPricingEntity
        {
            Id = id,
            CategoryId = categoryId,
            PricingTierId = pricingTierId,
            PriceUsd = priceUsd,
        };
        return pricing;
    }

    /// <summary>
    /// Updates the price for this pricing tier within the category.
    /// </summary>
    /// <param name="priceUsd">The new price in USD (must be >= 0).</param>
    internal void UpdatePrice(decimal priceUsd)
    {
        if (priceUsd < 0)
        {
            throw new ContentRuleException(ContentRuleCodes.CategoryPriceMustBeNonNegative);
        }

        PriceUsd = priceUsd;
    }
}
