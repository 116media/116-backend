using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Pricing behaviour of <see cref="CategoryEntity" />. Its state lives in <c>Entities/CategoryEntity.cs</c>.
/// </summary>
public sealed partial class CategoryEntity
{
    /// <summary>
    /// Sets the price for a tier within this category, adding the row or repricing the
    /// existing one. Returns false when the stored price already matches.
    /// </summary>
    /// <param name="pricingTierId">The tier being priced.</param>
    /// <param name="priceUsd">The price in USD.</param>
    /// <returns><c>true</c> if a row was added or repriced; otherwise <c>false</c>.</returns>
    public bool SetPricing(Guid pricingTierId, decimal priceUsd)
    {
        CategoryPricingEntity? existing = FindPricing(pricingTierId: pricingTierId);

        if (existing is null)
        {
            Pricing.Add(
                CategoryPricingEntity.Create(
                    id: Guid.NewGuid(),
                    categoryId: Id,
                    pricingTierId: pricingTierId,
                    priceUsd: priceUsd
                )
            );
            AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));

            return true;
        }

        if (existing.PriceUsd == priceUsd)
        {
            return false;
        }

        existing.UpdatePrice(priceUsd: priceUsd);
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));

        return true;
    }

    /// <summary>
    /// Removes the pricing row for a tier, reporting whether one was there.
    /// </summary>
    /// <param name="pricingTierId">The tier whose price is removed.</param>
    /// <returns><c>true</c> if a row was removed; otherwise <c>false</c>.</returns>
    public bool RemovePricing(Guid pricingTierId)
    {
        CategoryPricingEntity? existing = FindPricing(pricingTierId: pricingTierId);

        if (existing is null)
        {
            return false;
        }

        Pricing.Remove(existing);
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));

        return true;
    }

    /// <summary>
    /// Returns this category's pricing row for a tier, or null when the tier is unpriced.
    /// </summary>
    /// <param name="pricingTierId">The tier to look up.</param>
    /// <returns>The matching pricing row, or <c>null</c>.</returns>
    public CategoryPricingEntity? FindPricing(Guid pricingTierId)
    {
        return Pricing.FirstOrDefault(pricing => pricing.PricingTierId == pricingTierId);
    }
}
