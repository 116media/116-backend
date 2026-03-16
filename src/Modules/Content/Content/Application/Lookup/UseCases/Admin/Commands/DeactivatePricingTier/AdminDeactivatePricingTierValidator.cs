using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePricingTierCommand" /> ensuring a valid pricing tier ID is provided.
/// </summary>
public class AdminDeactivatePricingTierValidator : AbstractValidator<AdminDeactivatePricingTierCommand>
{
    /// <summary>
    /// Configures validation rules for pricing tier deactivation.
    /// </summary>
    public AdminDeactivatePricingTierValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Pricing tier ID");
    }
}
