using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Validator for the <see cref="AdminScheduleShootCommand" /> ensuring the shooting date is in the future.
/// </summary>
public class AdminScheduleShootValidator : AbstractValidator<AdminScheduleShootCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminScheduleShootValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Video validation error messages.</param>
    public AdminScheduleShootValidator(VideoErrorMessage i18n)
    {
        RuleFor(x => x.VideoId).IsValidGuid(i18n.Localizer);

        RuleFor(x => x.ShootingScheduledAt).ValidShootingScheduledAt(i18n);
    }
}
