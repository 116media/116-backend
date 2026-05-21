using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Article interaction validation error messages.</param>
    public PublicEditArticleCommentValidator(ArticleInteractionErrorMessage msg)
    {
        RuleFor(x => x.Body)
            .ValidCommentBody(
                commentBodyRequired: msg.CommentBodyRequired(),
                commentBodyTooLong: msg.CommentBodyTooLong(ContentConstants.MaxCommentBodyLength)
            );
    }
}
