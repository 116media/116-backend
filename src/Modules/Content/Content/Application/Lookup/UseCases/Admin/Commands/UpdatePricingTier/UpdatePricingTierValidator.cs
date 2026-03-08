using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Validator for the <see cref="UpdatePricingTierCommand" /> ensuring proper pricing tier data format.
/// </summary>
public class UpdatePricingTierValidator : AbstractValidator<UpdatePricingTierCommand>
{
    /// <summary>
    /// Configures validation rules for pricing tier update.
    /// </summary>
    public UpdatePricingTierValidator()
    {
        RuleFor(x => x.Id).ValidPricingTierId();
        RuleFor(x => x.Name).ValidPricingTierName();
        RuleFor(x => x.Description).ValidPricingTierDescription();
    }
}
