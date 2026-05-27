using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Validator for the <see cref="AdminActivatePricingTierCommand" /> ensuring a valid pricing tier ID is provided.
/// </summary>
public class AdminActivatePricingTierValidator : AbstractValidator<AdminActivatePricingTierCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivatePricingTierValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Pricing tier validation error messages.</param>
    public AdminActivatePricingTierValidator(PricingTierErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
