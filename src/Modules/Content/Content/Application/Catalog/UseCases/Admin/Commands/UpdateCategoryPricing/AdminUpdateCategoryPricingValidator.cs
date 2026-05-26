using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">Category validation error messages.</param>
    /// <param name="pricingTierMsg">Pricing tier validation error messages.</param>
    public AdminUpdateCategoryPricingValidator(CategoryErrorMessage i18n, PricingTierErrorMessage pricingTierMsg)
    {
        RuleFor(x => x.CategoryId).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.PricingTierId).IsValidGuid(pricingTierMsg.Localizer);
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd(i18n);
    }
}
