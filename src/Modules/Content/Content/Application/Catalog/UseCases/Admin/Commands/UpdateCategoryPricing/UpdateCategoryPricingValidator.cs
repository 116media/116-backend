using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Validator for the <see cref="UpdateCategoryPricingCommand" /> ensuring proper pricing data format.
/// </summary>
public class UpdateCategoryPricingValidator : AbstractValidator<UpdateCategoryPricingCommand>
{
    /// <summary>
    /// Configures validation rules for category pricing update.
    /// </summary>
    public UpdateCategoryPricingValidator()
    {
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
        RuleFor(x => x.PricingTierId).IsValidGuid("Pricing tier ID");
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd();
    }
}
