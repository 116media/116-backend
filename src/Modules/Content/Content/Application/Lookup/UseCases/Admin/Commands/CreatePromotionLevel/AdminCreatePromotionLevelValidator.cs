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
    /// <param name="msg">Promotion level validation error messages.</param>
    public AdminCreatePromotionLevelValidator(PromotionLevelErrorMessage msg)
    {
        RuleFor(x => x.Name).ValidPromotionLevelName(msg);
        RuleFor(x => x.DurationDays).ValidDurationDays(msg);
        RuleFor(x => x.PriceUsd).ValidPriceUsd(msg);
        When(x => x.SpotPriority.HasValue, () => RuleFor(x => x.SpotPriority).ValidSpotPriority(msg));
    }
}
