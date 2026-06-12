using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;

/// <summary>
/// Validator for the <see cref="AdminApproveArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminApproveArticleValidator : AbstractValidator<AdminApproveArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminApproveArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminApproveArticleValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Article.Msg.Localizer);
    }
}
