using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Validator for the <see cref="ArchiveArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class ArchiveArticleValidator : AbstractValidator<ArchiveArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article archiving.
    /// </summary>
    public ArchiveArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
