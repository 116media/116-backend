using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Validator for the <see cref="AdminForceUnpromoteArticleCommand" />.
/// </summary>
public class AdminForceUnpromoteArticleValidator : AbstractValidator<AdminForceUnpromoteArticleCommand>
{
    /// <summary>
    /// Configures validation rules for the force-unpromote article command.
    /// </summary>
    public AdminForceUnpromoteArticleValidator()
    {
        RuleFor(x => x.Slug).ValidArticleSlug();
        RuleFor(x => x.Reason).ValidUnpromoteReason();
    }
}
