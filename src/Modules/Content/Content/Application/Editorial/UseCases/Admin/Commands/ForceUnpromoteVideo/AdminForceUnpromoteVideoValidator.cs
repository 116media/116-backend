using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Validator for the <see cref="AdminForceUnpromoteVideoCommand" />.
/// </summary>
public class AdminForceUnpromoteVideoValidator : AbstractValidator<AdminForceUnpromoteVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminForceUnpromoteVideoValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="articleMsg">Article validation error messages.</param>
    /// <param name="videoMsg">Video validation error messages.</param>
    public AdminForceUnpromoteVideoValidator(ArticleErrorMessage articleMsg, VideoErrorMessage videoMsg)
    {
        RuleFor(x => x.Slug).ValidVideoSlug(videoMsg);
        RuleFor(x => x.Reason)
            .ValidUnpromoteReason(
                reasonRequired: articleMsg.RejectionReasonRequired(),
                reasonTooLong: articleMsg.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength)
            );
    }
}
