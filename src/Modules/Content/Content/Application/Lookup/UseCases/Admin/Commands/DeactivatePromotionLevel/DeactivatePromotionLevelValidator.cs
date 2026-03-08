using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Validator for the <see cref="DeactivatePromotionLevelCommand" /> ensuring a valid promotion level ID is provided.
/// </summary>
public class DeactivatePromotionLevelValidator : AbstractValidator<DeactivatePromotionLevelCommand>
{
    /// <summary>
    /// Configures validation rules for promotion level deactivation.
    /// </summary>
    public DeactivatePromotionLevelValidator()
    {
        RuleFor(x => x.Id).ValidPromotionLevelId();
    }
}
