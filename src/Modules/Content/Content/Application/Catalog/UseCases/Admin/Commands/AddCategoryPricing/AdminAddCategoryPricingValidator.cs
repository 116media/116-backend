using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminAddCategoryPricingValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="categoryMsg">Category validation error messages.</param>
    /// <param name="pricingTierMsg">Pricing tier validation error messages.</param>
    public AdminAddCategoryPricingValidator(CategoryErrorMessage categoryMsg, PricingTierErrorMessage pricingTierMsg)
    {
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
        RuleFor(x => x.PricingTierId).ValidPricingTierId(pricingTierMsg.NameRequired());
        RuleFor(x => x.PriceUsd).ValidCategoryPriceUsd(categoryMsg.PriceMustBeNonNegative());
    }
}
