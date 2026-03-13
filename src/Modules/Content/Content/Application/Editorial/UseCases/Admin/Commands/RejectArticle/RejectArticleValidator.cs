using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Validator for the <see cref="RejectArticleCommand" /> ensuring a rejection reason is provided.
/// </summary>
public class RejectArticleValidator : AbstractValidator<RejectArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article rejection.
    /// </summary>
    public RejectArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");

        RuleFor(x => x.Reason).ValidRejectionReason();
    }
}
