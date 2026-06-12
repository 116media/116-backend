using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Validator for the <see cref="AdminSubmitArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminSubmitArticleValidator : AbstractValidator<AdminSubmitArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminSubmitArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminSubmitArticleValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Article.Msg.Localizer);
    }
}
