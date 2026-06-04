using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Validator for the <see cref="AdminForceUnpromoteVideoCommand" />.
/// </summary>
public class AdminForceUnpromoteVideoValidator : AbstractValidator<AdminForceUnpromoteVideoCommand>
{
    /// <summary>
    /// Configures validation rules for the force-unpromote video command.
    /// </summary>
    public AdminForceUnpromoteVideoValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage("Video slug is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required.")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
