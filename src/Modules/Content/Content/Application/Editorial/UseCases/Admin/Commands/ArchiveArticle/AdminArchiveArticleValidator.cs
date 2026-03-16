using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Validator for the <see cref="AdminArchiveArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminArchiveArticleValidator : AbstractValidator<AdminArchiveArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article archiving.
    /// </summary>
    public AdminArchiveArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
