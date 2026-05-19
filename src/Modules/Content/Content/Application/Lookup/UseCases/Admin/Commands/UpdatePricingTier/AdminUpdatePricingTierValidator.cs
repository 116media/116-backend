using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminUpdatePricingTierValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Pricing tier validation error messages.</param>
    public AdminUpdatePricingTierValidator(PricingTierErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Pricing tier ID");
        RuleFor(x => x.Name).ValidPricingTierName(msg);
        RuleFor(x => x.Description).ValidPricingTierDescription(msg);
    }
}
