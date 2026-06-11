using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Pricing tier validation error messages.</param>
    public AdminCreatePricingTierValidator(PricingTierErrorMessage msg)
    {
        RuleFor(x => x.Name)
            .ValidPricingTierName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxPricingTierNameLength)
            );
        RuleFor(x => x.Description)
            .ValidPricingTierDescription(
                descriptionRequired: msg.DescriptionRequired(),
                descriptionTooLong: msg.DescriptionTooLong(ContentConstants.MaxPricingTierDescriptionLength)
            );
    }
}
