using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePricingTierCommand" /> ensuring a valid pricing tier ID is provided.
/// </summary>
public class AdminDeactivatePricingTierValidator : AbstractValidator<AdminDeactivatePricingTierCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivatePricingTierValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeactivatePricingTierValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.PricingTier.Msg.Localizer);
    }
}
