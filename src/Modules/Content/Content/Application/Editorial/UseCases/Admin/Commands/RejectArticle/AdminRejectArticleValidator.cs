using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Validator for the <see cref="AdminRejectArticleCommand" /> ensuring a rejection reason is provided.
/// </summary>
public class AdminRejectArticleValidator : AbstractValidator<AdminRejectArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRejectArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Article validation error messages.</param>
    public AdminRejectArticleValidator(ArticleErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");

        RuleFor(x => x.Reason)
            .ValidRejectionReason(
                reasonRequired: msg.RejectionReasonRequired(),
                reasonTooLong: msg.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength)
            );
    }
}
