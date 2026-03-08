using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;

/// <summary>
/// Validator for the <see cref="UpdatePromotionLevelCommand" /> ensuring proper promotion level data format.
/// </summary>
public class UpdatePromotionLevelValidator : AbstractValidator<UpdatePromotionLevelCommand>
{
    /// <summary>
    /// Configures validation rules for promotion level update.
    /// </summary>
    public UpdatePromotionLevelValidator()
    {
        RuleFor(x => x.Id).ValidPromotionLevelId();
        RuleFor(x => x.Name).ValidPromotionLevelName();
        RuleFor(x => x.DurationDays).ValidDurationDays();
        RuleFor(x => x.PriceUsd).ValidPriceUsd();
    }
}
