using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Validator for the <see cref="AdminSubmitArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminSubmitArticleValidator : AbstractValidator<AdminSubmitArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article submission.
    /// </summary>
    public AdminSubmitArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
