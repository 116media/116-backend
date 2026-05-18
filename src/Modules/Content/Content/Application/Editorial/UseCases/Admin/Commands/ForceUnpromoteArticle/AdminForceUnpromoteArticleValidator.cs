using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Validator for the <see cref="AdminForceUnpromoteArticleCommand" />.
/// </summary>
public class AdminForceUnpromoteArticleValidator : AbstractValidator<AdminForceUnpromoteArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminForceUnpromoteArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Article validation error messages.</param>
    public AdminForceUnpromoteArticleValidator(ArticleErrorMessage msg)
    {
        RuleFor(x => x.Slug).ValidArticleSlug(msg);
        RuleFor(x => x.Reason).ValidUnpromoteReason(msg);
    }
}
