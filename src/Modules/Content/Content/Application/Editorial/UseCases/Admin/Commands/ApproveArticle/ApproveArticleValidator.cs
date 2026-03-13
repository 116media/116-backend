using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;

/// <summary>
/// Validator for the <see cref="ApproveArticleCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class ApproveArticleValidator : AbstractValidator<ApproveArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article approval.
    /// </summary>
    public ApproveArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
