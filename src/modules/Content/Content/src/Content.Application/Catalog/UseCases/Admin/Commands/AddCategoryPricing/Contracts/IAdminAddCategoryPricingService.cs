using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.Contracts;

/// <summary>
/// The pricing row just added and the tier it names.
/// </summary>
/// <param name="Pricing">The new pricing row.</param>
/// <param name="Tier">The tier the row prices.</param>
public record CategoryPricingData(CategoryPricingEntity Pricing, PricingTierEntity Tier);

/// <summary>
/// Resolves and applies a category pricing row: loads the category and the tier, gates an
/// inactive tier and a duplicate row, and sets the price through the category aggregate.
/// </summary>
public interface IAdminAddCategoryPricingService
{
    /// <summary>
    /// Adds the pricing row, throwing the localized error when the tier is inactive or the row
    /// already exists. The caller owns the commit.
    /// </summary>
    /// <param name="categoryId">The category being priced.</param>
    /// <param name="pricingTierId">The tier being priced.</param>
    /// <param name="priceUsd">The price in USD.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<CategoryPricingData> AddAsync(
        Guid categoryId,
        Guid pricingTierId,
        decimal priceUsd,
        CancellationToken cancellationToken
    );
}
