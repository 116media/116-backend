using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Validator for the <see cref="ScheduleShootCommand" /> ensuring the shooting date is in the future.
/// </summary>
public class ScheduleShootValidator : AbstractValidator<ScheduleShootCommand>
{
    /// <summary>
    /// Configures validation rules for scheduling a video shoot.
    /// </summary>
    public ScheduleShootValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");

        RuleFor(x => x.ShootingScheduledAt).ValidShootingScheduledAt();
    }
}
