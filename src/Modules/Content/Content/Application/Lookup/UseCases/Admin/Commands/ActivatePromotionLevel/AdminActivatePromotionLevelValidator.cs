using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Validator for the <see cref="AdminActivatePromotionLevelCommand" /> ensuring a valid promotion level ID is provided.
/// </summary>
public class AdminActivatePromotionLevelValidator : AbstractValidator<AdminActivatePromotionLevelCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivatePromotionLevelValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Promotion level validation error messages.</param>
    public AdminActivatePromotionLevelValidator(PromotionLevelErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
