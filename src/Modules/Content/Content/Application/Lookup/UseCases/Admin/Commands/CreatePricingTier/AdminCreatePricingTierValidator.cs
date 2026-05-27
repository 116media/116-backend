using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Validator for the <see cref="AdminCreatePricingTierCommand" /> ensuring proper pricing tier data format.
/// </summary>
public class AdminCreatePricingTierValidator : AbstractValidator<AdminCreatePricingTierCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreatePricingTierValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Pricing tier validation error messages.</param>
    public AdminCreatePricingTierValidator(PricingTierErrorMessage i18n)
    {
        RuleFor(x => x.Name).ValidPricingTierName(i18n);
        RuleFor(x => x.Description).ValidPricingTierDescription(i18n);
    }
}
