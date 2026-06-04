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
        RuleFor(x => x.Slug).NotEmpty().WithMessage("Article slug is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required.")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
