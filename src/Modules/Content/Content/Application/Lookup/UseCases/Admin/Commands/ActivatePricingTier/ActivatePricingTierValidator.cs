using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Validator for the <see cref="ActivatePricingTierCommand" /> ensuring a valid pricing tier ID is provided.
/// </summary>
public class ActivatePricingTierValidator : AbstractValidator<ActivatePricingTierCommand>
{
    /// <summary>
    /// Configures validation rules for pricing tier activation.
    /// </summary>
    public ActivatePricingTierValidator()
    {
        RuleFor(x => x.Id).ValidPricingTierId();
    }
}
