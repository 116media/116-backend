using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Validator for the <see cref="PublicAddArticleCommentCommand" />.
/// </summary>
public class PublicAddArticleCommentValidator : AbstractValidator<PublicAddArticleCommentCommand>
{
    /// <summary>
    /// Configures validation rules for adding an article comment.
    /// </summary>
    public PublicAddArticleCommentValidator()
    {
        RuleFor(x => x.Body).ValidCommentBody();
    }
}
