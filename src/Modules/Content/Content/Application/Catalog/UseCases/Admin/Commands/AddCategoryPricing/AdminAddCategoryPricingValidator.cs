using _116.Content.Application.Shared.Errors.Facade;
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
    /// Initializes a new instance of <see cref="AdminAddCategoryPricingValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminAddCategoryPricingValidator(ContentI18n i18n)
    {
        RuleFor(x => x.CategoryId).IsValidGuid(i18n.Category.Msg.Localizer);
        RuleFor(x => x.PricingTierId).ValidPricingTierId(i18n.PricingTier.Msg);
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd(i18n.Category.Msg);
    }
}
