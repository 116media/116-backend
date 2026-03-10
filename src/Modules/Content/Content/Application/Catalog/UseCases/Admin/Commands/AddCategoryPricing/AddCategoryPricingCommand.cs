using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Command for adding a pricing tier to a category.
/// </summary>
/// <param name="CategoryId">The identifier of the category to configure pricing for.</param>
/// <param name="PricingTierId">The identifier of the pricing tier to attach.</param>
/// <param name="PriceUsd">The price in USD for this tier within the category.</param>
public record AddCategoryPricingCommand(Guid CategoryId, Guid PricingTierId, decimal PriceUsd)
    : ICommand<AddCategoryPricingResult>;

/// <summary>
/// Result of the <see cref="AddCategoryPricingCommand" /> containing the created pricing details.
/// </summary>
/// <param name="Pricing">The created category pricing information.</param>
public record AddCategoryPricingResult(CategoryPricingDto Pricing);
