using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Promotion level validation error messages.</param>
    public AdminUpdatePromotionLevelValidator(PromotionLevelErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Promotion level ID");
        RuleFor(x => x.Name)
            .ValidPromotionLevelName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxPromotionLevelNameLength)
            );
        RuleFor(x => x.DurationDays).ValidDurationDays(msg.DurationMustBePositive());
        RuleFor(x => x.PriceUsd).ValidPriceUsd(msg.PriceMustBeNonNegative());
        When(
            x => x.SpotPriority.HasValue,
            () => RuleFor(x => x.SpotPriority).ValidSpotPriority(msg.InvalidSpotPriority())
        );
    }
}
