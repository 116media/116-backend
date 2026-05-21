using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Validator for the <see cref="AdminUpdateArticleTagsCommand" /> ensuring a valid article ID
/// and that each tag name satisfies the tag name constraints.
/// </summary>
public class AdminUpdateArticleTagsValidator : AbstractValidator<AdminUpdateArticleTagsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateArticleTagsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Tag validation error messages.</param>
    public AdminUpdateArticleTagsValidator(TagErrorMessage msg)
    {
        RuleFor(x => x.ArticleId).IsValidGuid("Article ID");
        RuleForEach(x => x.TagNames)
            .ValidTagNameItem(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxTagNameLength)
            );
    }
}
