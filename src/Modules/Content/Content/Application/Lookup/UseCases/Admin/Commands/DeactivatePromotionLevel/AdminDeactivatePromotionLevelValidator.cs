using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePromotionLevelCommand" /> ensuring a valid promotion level ID is provided.
/// </summary>
public class AdminDeactivatePromotionLevelValidator : AbstractValidator<AdminDeactivatePromotionLevelCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivatePromotionLevelValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeactivatePromotionLevelValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.PromotionLevel.Msg.Localizer);
    }
}
