using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Validator for the <see cref="RemoveCategoryPricingCommand" /> ensuring valid IDs are provided.
/// </summary>
public class RemoveCategoryPricingValidator : AbstractValidator<RemoveCategoryPricingCommand>
{
    /// <summary>
    /// Configures validation rules for category pricing removal.
    /// </summary>
    public RemoveCategoryPricingValidator()
    {
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
    }
}
