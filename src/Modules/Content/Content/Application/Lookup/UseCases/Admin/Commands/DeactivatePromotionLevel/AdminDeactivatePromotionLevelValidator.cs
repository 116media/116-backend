using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePromotionLevelCommand" /> ensuring a valid promotion level ID is provided.
/// </summary>
public class AdminDeactivatePromotionLevelValidator : AbstractValidator<AdminDeactivatePromotionLevelCommand>
{
    /// <summary>
    /// Configures validation rules for promotion level deactivation.
    /// </summary>
    public AdminDeactivatePromotionLevelValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Promotion level ID");
    }
}
