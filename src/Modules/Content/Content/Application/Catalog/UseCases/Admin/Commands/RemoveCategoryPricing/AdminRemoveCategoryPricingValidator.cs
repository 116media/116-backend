using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Validator for the <see cref="AdminRemoveCategoryPricingCommand" /> ensuring valid IDs are provided.
/// </summary>
public class AdminRemoveCategoryPricingValidator : AbstractValidator<AdminRemoveCategoryPricingCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRemoveCategoryPricingValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminRemoveCategoryPricingValidator(ContentI18n i18n)
    {
        RuleFor(x => x.CategoryId).IsValidGuid(i18n.Category.Msg.Localizer);
        RuleFor(x => x.PricingTierId).IsValidGuid(i18n.PricingTier.Msg.Localizer);
    }
}
