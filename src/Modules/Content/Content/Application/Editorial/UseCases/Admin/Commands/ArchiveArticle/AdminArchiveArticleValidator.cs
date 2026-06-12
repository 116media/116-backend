using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Validator for the <see cref="AdminArchiveArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminArchiveArticleValidator : AbstractValidator<AdminArchiveArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminArchiveArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Article validation error messages.</param>
    public AdminArchiveArticleValidator(ArticleErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
