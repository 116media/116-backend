using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="CategoryPricingEntity" />. Its state lives in <c>Entities/CategoryPricingEntity.cs</c>.
/// </summary>
public partial class CategoryPricingEntity
{
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
