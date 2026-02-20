using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Validator for the <see cref="ActivatePromotionLevelCommand" /> ensuring a valid promotion level ID is provided.
/// </summary>
public class ActivatePromotionLevelValidator : AbstractValidator<ActivatePromotionLevelCommand>
{
    /// <summary>
    /// Configures validation rules for promotion level activation.
    /// </summary>
    public ActivatePromotionLevelValidator()
    {
        RuleFor(x => x.Id).ValidPromotionLevelId();
    }
}
