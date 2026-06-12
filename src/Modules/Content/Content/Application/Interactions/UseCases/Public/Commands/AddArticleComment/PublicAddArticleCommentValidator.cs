using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Validator for the <see cref="PublicAddArticleCommentCommand" />.
/// </summary>
public class PublicAddArticleCommentValidator : AbstractValidator<PublicAddArticleCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicAddArticleCommentValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Article interaction validation error messages.</param>
    public PublicAddArticleCommentValidator(ArticleInteractionErrorMessage i18n)
    {
        RuleFor(x => x.Body).ValidCommentBody(i18n);
    }
}
