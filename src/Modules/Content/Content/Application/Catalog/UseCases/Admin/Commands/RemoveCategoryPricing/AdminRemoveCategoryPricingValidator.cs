using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Validator for the <see cref="AdminRemoveCategoryPricingCommand" /> ensuring valid IDs are provided.
/// </summary>
public class AdminRemoveCategoryPricingValidator : AbstractValidator<AdminRemoveCategoryPricingCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRemoveCategoryPricingValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="categoryMsg">Category validation error messages.</param>
    /// <param name="pricingTierMsg">Pricing tier validation error messages.</param>
    public AdminRemoveCategoryPricingValidator(CategoryErrorMessage categoryMsg, PricingTierErrorMessage pricingTierMsg)
    {
        RuleFor(x => x.CategoryId).IsValidGuid(categoryMsg.Localizer);
        RuleFor(x => x.PricingTierId).IsValidGuid(pricingTierMsg.Localizer);
    }
}
