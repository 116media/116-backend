using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;

/// <summary>
/// Validator for the <see cref="PublicEditArticleCommentCommand" />.
/// </summary>
public class PublicEditArticleCommentValidator : AbstractValidator<PublicEditArticleCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicEditArticleCommentValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicEditArticleCommentValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Body).ValidCommentBody(i18n.ArticleInteraction.Msg);
    }
}
