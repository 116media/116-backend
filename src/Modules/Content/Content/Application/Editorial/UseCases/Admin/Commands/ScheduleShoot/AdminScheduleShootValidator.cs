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
    /// Configures validation rules for scheduling a video shoot.
    /// </summary>
    public AdminScheduleShootValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");

        RuleFor(x => x.ShootingScheduledAt).ValidShootingScheduledAt();
    }
}
