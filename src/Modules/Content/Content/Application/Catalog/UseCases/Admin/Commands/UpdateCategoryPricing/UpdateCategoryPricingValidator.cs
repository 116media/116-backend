using _116.Content.Application.Shared.Validators;
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
        RuleFor(x => x.CategoryId).ValidCategoryId();
        RuleFor(x => x.PricingTierId).ValidPricingTierId();
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd();
    }
}
