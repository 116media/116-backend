using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminActivatePricingTierValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.PricingTier.Msg.Localizer);
    }
}
