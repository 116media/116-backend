using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Validator for the <see cref="AdminCreatePromotionLevelCommand" /> ensuring proper promotion level data format.
/// </summary>
public class AdminCreatePromotionLevelValidator : AbstractValidator<AdminCreatePromotionLevelCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreatePromotionLevelValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Promotion level validation error messages.</param>
    public AdminCreatePromotionLevelValidator(PromotionLevelErrorMessage i18n)
    {
        RuleFor(x => x.Name).ValidPromotionLevelName(i18n);
        RuleFor(x => x.DurationDays).ValidDurationDays(i18n);
        RuleFor(x => x.PriceUsd).ValidPriceUsd(i18n);
        When(x => x.SpotPriority.HasValue, () => RuleFor(x => x.SpotPriority).ValidSpotPriority(i18n));
    }
}
