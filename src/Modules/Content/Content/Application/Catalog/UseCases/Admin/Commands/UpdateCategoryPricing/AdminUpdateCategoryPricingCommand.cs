using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Command for updating the price of an existing pricing tier within a category.
/// </summary>
/// <param name="CategoryId">The identifier of the category.</param>
/// <param name="PricingTierId">The identifier of the pricing tier to update.</param>
/// <param name="PriceUsd">The new price in USD.</param>
public record AdminUpdateCategoryPricingCommand(string CategoryId, string PricingTierId, decimal PriceUsd)
    : ICommand<AdminUpdateCategoryPricingResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateCategoryPricingCommand" /> containing the updated pricing details.
/// </summary>
/// <param name="Pricing">The updated category pricing information.</param>
public record AdminUpdateCategoryPricingResult(CategoryPricingDto Pricing);
