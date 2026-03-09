using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Validator for the <see cref="AdminAddCategoryPricingCommand" /> ensuring proper pricing data format.
/// </summary>
public class AdminAddCategoryPricingValidator : AbstractValidator<AdminAddCategoryPricingCommand>
{
    /// <summary>
    /// Configures validation rules for category pricing creation.
    /// </summary>
    public AdminAddCategoryPricingValidator()
    {
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
        RuleFor(x => x.PricingTierId).ValidPricingTierId();
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd();
    }
}
