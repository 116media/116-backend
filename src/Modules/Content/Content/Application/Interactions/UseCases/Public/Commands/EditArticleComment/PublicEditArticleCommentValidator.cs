using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;

/// <summary>
/// Validator for the <see cref="PublicEditArticleCommentCommand" />.
/// </summary>
public class PublicEditArticleCommentValidator : AbstractValidator<PublicEditArticleCommentCommand>
{
    /// <summary>
    /// Configures validation rules for editing an article comment.
    /// </summary>
    public PublicEditArticleCommentValidator()
    {
        RuleFor(x => x.Body).ValidCommentBody();
    }
}
