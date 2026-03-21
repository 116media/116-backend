using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Validator for the <see cref="AdminRemoveCategoryPricingCommand" /> ensuring valid IDs are provided.
/// </summary>
public class AdminRemoveCategoryPricingValidator : AbstractValidator<AdminRemoveCategoryPricingCommand>
{
    /// <summary>
    /// Configures validation rules for category pricing removal.
    /// </summary>
    public AdminRemoveCategoryPricingValidator()
    {
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
        RuleFor(x => x.PricingTierId).IsValidGuid("Pricing Tier ID");
    }
}
