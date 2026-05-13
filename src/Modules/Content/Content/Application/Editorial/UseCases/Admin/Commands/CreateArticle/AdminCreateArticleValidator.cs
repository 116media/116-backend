using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Validator for the <see cref="AdminCreateArticleCommand" /> ensuring proper article draft creation data.
/// </summary>
public class AdminCreateArticleValidator : AbstractValidator<AdminCreateArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article draft creation.
    /// </summary>
    public AdminCreateArticleValidator()
    {
        RuleFor(x => x.CategoryId).ValidArticleCategoryId();
        RuleFor(x => x.Title).ValidArticleTitle();
        RuleFor(x => x.Slug).ValidArticleSlug();

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId());
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId());
    }
}
