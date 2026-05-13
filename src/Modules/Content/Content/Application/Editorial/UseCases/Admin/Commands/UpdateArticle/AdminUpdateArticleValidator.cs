using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Validator for the <see cref="AdminUpdateArticleCommand" /> ensuring all editable article fields are valid.
/// </summary>
public class AdminUpdateArticleValidator : AbstractValidator<AdminUpdateArticleCommand>
{
    /// <summary>
    /// Configures validation rules for the full article update.
    /// </summary>
    public AdminUpdateArticleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");

        RuleFor(x => x.CategoryId).ValidArticleCategoryId();

        RuleFor(x => x.Title).ValidArticleTitle();

        RuleFor(x => x.Slug).ValidArticleSlug();

        RuleFor(x => x.Headline).ValidArticleHeadline();

        RuleFor(x => x.Body).ValidArticleBody();

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId());
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId());

        RuleFor(x => x.MetaTitle).ValidOptionalMetaTitle(x => x.MetaTitle is not null);

        RuleFor(x => x.MetaDescription).ValidOptionalMetaDescription(x => x.MetaDescription is not null);
    }
}
