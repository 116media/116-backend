using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Validator for the <see cref="AdminRejectVideoCommand" /> ensuring a rejection reason is provided.
/// </summary>
public class AdminRejectVideoValidator : AbstractValidator<AdminRejectVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video rejection.
    /// </summary>
    public AdminRejectVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");

        RuleFor(x => x.Reason).ValidRejectionReason();
    }
}
