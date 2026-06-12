using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">Article interaction validation error messages.</param>
    public PublicEditArticleCommentValidator(ArticleInteractionErrorMessage i18n)
    {
        RuleFor(x => x.Body).ValidCommentBody(i18n);
    }
}
