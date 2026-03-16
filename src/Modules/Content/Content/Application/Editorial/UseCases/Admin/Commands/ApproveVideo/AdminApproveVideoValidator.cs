using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;

/// <summary>
/// Validator for the <see cref="AdminApproveVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminApproveVideoValidator : AbstractValidator<AdminApproveVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video approval.
    /// </summary>
    public AdminApproveVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
