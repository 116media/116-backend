using _116.Content.Application.Shared.Errors.Facade;
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
    /// Initializes a new instance of <see cref="AdminUpdateCategoryPricingValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateCategoryPricingValidator(ContentI18n i18n)
    {
        RuleFor(x => x.CategoryId).IsValidGuid(i18n.Category.Msg.Localizer);
        RuleFor(x => x.PricingTierId).IsValidGuid(i18n.PricingTier.Msg.Localizer);
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd(i18n.Category.Msg);
    }
}
