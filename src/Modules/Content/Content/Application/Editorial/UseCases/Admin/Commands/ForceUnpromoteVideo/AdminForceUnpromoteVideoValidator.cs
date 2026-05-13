using _116.Content.Application.Shared.Validators;
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
        RuleFor(x => x.Slug).ValidVideoSlug();
        RuleFor(x => x.Reason).ValidUnpromoteReason();
    }
}
