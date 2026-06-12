using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Validator for the <see cref="AdminDeleteArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminDeleteArticleValidator : AbstractValidator<AdminDeleteArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeleteArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeleteArticleValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Article.Msg.Localizer);
    }
}
