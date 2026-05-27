using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Validator for the <see cref="AdminForceUnpromoteArticleCommand" />.
/// </summary>
public class AdminForceUnpromoteArticleValidator : AbstractValidator<AdminForceUnpromoteArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminForceUnpromoteArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Article validation error messages.</param>
    public AdminForceUnpromoteArticleValidator(ArticleErrorMessage i18n)
    {
        RuleFor(x => x.Slug).ValidArticleSlug(i18n);
        RuleFor(x => x.Reason)
            .ValidUnpromoteReason(
                reasonRequired: i18n.RejectionReasonRequired(),
                reasonTooLong: i18n.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength)
            );
    }
}
