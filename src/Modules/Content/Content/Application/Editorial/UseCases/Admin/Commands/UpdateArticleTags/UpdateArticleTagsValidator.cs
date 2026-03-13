using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Validator for the <see cref="UpdateArticleTagsCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class UpdateArticleTagsValidator : AbstractValidator<UpdateArticleTagsCommand>
{
    /// <summary>
    /// Configures validation rules for article tags update.
    /// </summary>
    public UpdateArticleTagsValidator()
    {
        RuleFor(x => x.ArticleId).IsValidGuid("Article ID");
    }
}
