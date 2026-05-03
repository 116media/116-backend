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
    /// Configures validation rules for article tags update.
    /// </summary>
    public AdminUpdateArticleTagsValidator()
    {
        RuleFor(x => x.ArticleId).IsValidGuid("Article ID");
        RuleForEach(x => x.TagNames)
            .NotEmpty()
            .WithMessage("Tag name is required.")
            .MaximumLength(ContentConstants.MaxTagNameLength)
            .WithMessage($"Tag name must not exceed {ContentConstants.MaxTagNameLength} characters.");
    }
}
