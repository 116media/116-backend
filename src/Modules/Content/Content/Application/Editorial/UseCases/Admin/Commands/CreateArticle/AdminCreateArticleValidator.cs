using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Validator for the <see cref="AdminCreateArticleCommand" /> ensuring proper article draft creation data.
/// </summary>
public class AdminCreateArticleValidator : AbstractValidator<AdminCreateArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateArticleValidator(ContentI18n i18n)
    {
        RuleFor(x => x.CategoryId).ValidArticleCategoryId(i18n.Article.Msg.CategoryIdRequired());
        RuleFor(x => x.Title).ValidArticleTitle(i18n.Article.Msg);
        RuleFor(x => x.Slug).ValidArticleSlug(i18n.Article.Msg);

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId(i18n.ContentOrder.Msg));
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId(i18n.Customer.Msg));
    }
}
