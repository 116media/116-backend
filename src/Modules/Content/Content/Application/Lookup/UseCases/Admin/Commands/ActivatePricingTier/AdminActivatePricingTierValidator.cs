using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Validator for the <see cref="AdminActivatePricingTierCommand" /> ensuring a valid pricing tier ID is provided.
/// </summary>
public class AdminActivatePricingTierValidator : AbstractValidator<AdminActivatePricingTierCommand>
{
    /// <summary>
    /// Configures validation rules for pricing tier activation.
    /// </summary>
    public AdminActivatePricingTierValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Pricing tier ID");
    }
}
