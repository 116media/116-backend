using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Validator for the <see cref="AdminActivatePromotionLevelCommand" /> ensuring a valid promotion level ID is provided.
/// </summary>
public class AdminActivatePromotionLevelValidator : AbstractValidator<AdminActivatePromotionLevelCommand>
{
    /// <summary>
    /// Configures validation rules for promotion level activation.
    /// </summary>
    public AdminActivatePromotionLevelValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Promotion level ID");
    }
}
