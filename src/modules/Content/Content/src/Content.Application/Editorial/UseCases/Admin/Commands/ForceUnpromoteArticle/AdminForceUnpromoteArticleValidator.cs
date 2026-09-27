using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminForceUnpromoteArticleValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Slug).ValidArticleSlug(i18n.Article.Msg);
        RuleFor(x => x.Reason)
            .ValidUnpromoteReason(
                reasonRequired: i18n.Article.Msg.RejectionReasonRequired(),
                reasonTooLong: i18n.Article.Msg.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength)
            );
    }
}
