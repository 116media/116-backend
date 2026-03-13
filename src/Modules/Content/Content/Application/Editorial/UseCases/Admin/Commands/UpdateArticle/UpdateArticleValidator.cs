using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Validator for the <see cref="UpdateArticleCommand" /> ensuring proper article content data.
/// </summary>
public class UpdateArticleValidator : AbstractValidator<UpdateArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article content update.
    /// </summary>
    public UpdateArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");

        RuleFor(x => x.Headline).ValidArticleHeadline();

        RuleFor(x => x.Body).ValidArticleBody();
    }
}
