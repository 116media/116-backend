using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Validator for the <see cref="CreatePromotionLevelCommand" /> ensuring proper promotion level data format.
/// </summary>
public class CreatePromotionLevelValidator : AbstractValidator<CreatePromotionLevelCommand>
{
    /// <summary>
    /// Configures validation rules for promotion level creation.
    /// </summary>
    public CreatePromotionLevelValidator()
    {
        RuleFor(x => x.Name).ValidPromotionLevelName();
        RuleFor(x => x.DurationDays).ValidDurationDays();
        RuleFor(x => x.PriceUsd).ValidPriceUsd();
    }
}
