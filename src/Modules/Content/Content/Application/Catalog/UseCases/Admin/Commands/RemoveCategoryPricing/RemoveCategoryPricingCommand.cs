using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Command for removing a pricing tier from a category.
/// </summary>
/// <param name="CategoryId">The identifier of the category.</param>
/// <param name="PricingTierId">The identifier of the pricing tier to remove.</param>
public record RemoveCategoryPricingCommand(string CategoryId, Guid PricingTierId)
    : ICommand<RemoveCategoryPricingResult>;

/// <summary>
/// Result of the <see cref="RemoveCategoryPricingCommand" /> containing the remaining pricing for the category.
/// </summary>
/// <param name="Pricing">The remaining pricing tiers for the category after removal.</param>
/// <param name="IsSuccess">Indicates whether the pricing tier was successfully removed.</param>
public record RemoveCategoryPricingResult(IReadOnlyList<CategoryPricingDto> Pricing, bool IsSuccess);
