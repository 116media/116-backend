using _116.Content.Application.Shared.Errors.Facade;
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
    /// Initializes a new instance of <see cref="AdminUpdatePromotionLevelValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdatePromotionLevelValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.PromotionLevel.Msg.Localizer);
        RuleFor(x => x.Name).ValidPromotionLevelName(i18n.PromotionLevel.Msg);
        RuleFor(x => x.DurationDays).ValidDurationDays(i18n.PromotionLevel.Msg);
        RuleFor(x => x.PriceUsd).ValidPriceUsd(i18n.PromotionLevel.Msg);
        When(
            x => x.SpotPriority.HasValue,
            () => RuleFor(x => x.SpotPriority).ValidSpotPriority(i18n.PromotionLevel.Msg)
        );
    }
}
