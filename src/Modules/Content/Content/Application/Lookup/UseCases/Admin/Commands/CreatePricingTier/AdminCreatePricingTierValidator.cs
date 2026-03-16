using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Validator for the <see cref="AdminCreatePricingTierCommand" /> ensuring proper pricing tier data format.
/// </summary>
public class AdminCreatePricingTierValidator : AbstractValidator<AdminCreatePricingTierCommand>
{
    /// <summary>
    /// Configures validation rules for pricing tier creation.
    /// </summary>
    public AdminCreatePricingTierValidator()
    {
        RuleFor(x => x.Name).ValidPricingTierName();
        RuleFor(x => x.Description).ValidPricingTierDescription();
    }
}
