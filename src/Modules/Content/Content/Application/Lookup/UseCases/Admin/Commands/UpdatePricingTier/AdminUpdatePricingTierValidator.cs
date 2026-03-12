using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Validator for the <see cref="AdminUpdatePricingTierCommand" /> ensuring proper pricing tier data format.
/// </summary>
public class AdminUpdatePricingTierValidator : AbstractValidator<AdminUpdatePricingTierCommand>
{
    /// <summary>
    /// Configures validation rules for pricing tier update.
    /// </summary>
    public AdminUpdatePricingTierValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Pricing tier ID");
        RuleFor(x => x.Name).ValidPricingTierName();
        RuleFor(x => x.Description).ValidPricingTierDescription();
    }
}
