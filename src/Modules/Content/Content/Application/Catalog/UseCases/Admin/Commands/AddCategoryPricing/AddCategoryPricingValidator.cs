using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Validator for the <see cref="AddCategoryPricingCommand" /> ensuring proper pricing data format.
/// </summary>
public class AddCategoryPricingValidator : AbstractValidator<AddCategoryPricingCommand>
{
    /// <summary>
    /// Configures validation rules for category pricing creation.
    /// </summary>
    public AddCategoryPricingValidator()
    {
        RuleFor(x => x.CategoryId).ValidCategoryId();
        RuleFor(x => x.PricingTierId).ValidPricingTierId();
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd();
    }
}
