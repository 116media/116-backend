using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Validator for the <see cref="AdminDeleteArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminDeleteArticleValidator : AbstractValidator<AdminDeleteArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article deletion.
    /// </summary>
    public AdminDeleteArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
