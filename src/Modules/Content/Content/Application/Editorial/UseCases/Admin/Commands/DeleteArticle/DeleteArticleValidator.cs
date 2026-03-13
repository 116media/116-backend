using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Validator for the <see cref="DeleteArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class DeleteArticleValidator : AbstractValidator<DeleteArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article deletion.
    /// </summary>
    public DeleteArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
