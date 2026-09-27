using _116.Content.Application.Shared.Errors.Facade;
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
    /// Initializes a new instance of <see cref="AdminForceUnpromoteVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminForceUnpromoteVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Slug).ValidVideoSlug(i18n.Video.Msg);
        RuleFor(x => x.Reason)
            .ValidUnpromoteReason(
                reasonRequired: i18n.Article.Msg.RejectionReasonRequired(),
                reasonTooLong: i18n.Article.Msg.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength)
            );
    }
}
