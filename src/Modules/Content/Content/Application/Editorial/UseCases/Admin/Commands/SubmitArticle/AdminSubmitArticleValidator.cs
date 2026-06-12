using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Validator for the <see cref="AdminSubmitArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminSubmitArticleValidator : AbstractValidator<AdminSubmitArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminSubmitArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Article validation error messages.</param>
    public AdminSubmitArticleValidator(ArticleErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
