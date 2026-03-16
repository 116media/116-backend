using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle;

/// <summary>
/// Validator for the <see cref="AdminPublishArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminPublishArticleValidator : AbstractValidator<AdminPublishArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article publishing.
    /// </summary>
    public AdminPublishArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
