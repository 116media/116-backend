using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;

/// <summary>
/// Validator for the <see cref="AdminUpdatePromotionLevelCommand" /> ensuring proper promotion level data format.
/// </summary>
public class AdminUpdatePromotionLevelValidator : AbstractValidator<AdminUpdatePromotionLevelCommand>
{
    /// <summary>
    /// Configures validation rules for promotion level update.
    /// </summary>
    public AdminUpdatePromotionLevelValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Promotion level ID");
        RuleFor(x => x.Name).ValidPromotionLevelName();
        RuleFor(x => x.DurationDays).ValidDurationDays();
        RuleFor(x => x.PriceUsd).ValidPriceUsd();
    }
}
