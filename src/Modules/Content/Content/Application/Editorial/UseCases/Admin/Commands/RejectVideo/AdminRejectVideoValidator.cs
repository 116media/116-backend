using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Validator for the <see cref="AdminRejectVideoCommand" /> ensuring a rejection reason is provided.
/// </summary>
public class AdminRejectVideoValidator : AbstractValidator<AdminRejectVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRejectVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Article validation error messages.</param>
    public AdminRejectVideoValidator(ArticleErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");

        RuleFor(x => x.Reason)
            .ValidRejectionReason(
                reasonRequired: msg.RejectionReasonRequired(),
                reasonTooLong: msg.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength)
            );
    }
}
