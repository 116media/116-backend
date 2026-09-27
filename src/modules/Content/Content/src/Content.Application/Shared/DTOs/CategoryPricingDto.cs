namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a single pricing tier configured for a category.
/// </summary>
/// <param name="TierId">The unique identifier of the pricing tier.</param>
/// <param name="TierName">The display name of the pricing tier.</param>
/// <param name="PriceUsd">The price in USD for this tier within the category.</param>
public record CategoryPricingDto(Guid TierId, string TierName, decimal PriceUsd);
