using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminScheduleShootValidator(ContentI18n i18n)
    {
        RuleFor(x => x.VideoId).IsValidGuid(i18n.Video.Msg.Localizer);

        RuleFor(x => x.ShootingScheduledAt).ValidShootingScheduledAt(i18n.Video.Msg);
    }
}
