using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Validator for the <see cref="SubmitArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class SubmitArticleValidator : AbstractValidator<SubmitArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article submission.
    /// </summary>
    public SubmitArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
