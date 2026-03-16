using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Validator for the <see cref="AdminUpdateCategoryPricingCommand" /> ensuring proper pricing data format.
/// </summary>
public class AdminUpdateCategoryPricingValidator : AbstractValidator<AdminUpdateCategoryPricingCommand>
{
    /// <summary>
    /// Configures validation rules for category pricing update.
    /// </summary>
    public AdminUpdateCategoryPricingValidator()
    {
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
        RuleFor(x => x.PricingTierId).IsValidGuid("Pricing tier ID");
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd();
    }
}
