using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Validator for the <see cref="AdminCreateArticleCommand" /> ensuring proper article draft creation data.
/// </summary>
public class AdminCreateArticleValidator : AbstractValidator<AdminCreateArticleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateArticleValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="articleMsg">Article validation error messages.</param>
    /// <param name="orderMsg">Content order validation error messages.</param>
    /// <param name="customerMsg">Customer validation error messages.</param>
    public AdminCreateArticleValidator(
        ArticleErrorMessage articleMsg,
        ContentOrderErrorMessage orderMsg,
        CustomerErrorMessage customerMsg
    )
    {
        RuleFor(x => x.CategoryId).ValidArticleCategoryId(articleMsg);
        RuleFor(x => x.Title).ValidArticleTitle(articleMsg);
        RuleFor(x => x.Slug).ValidArticleSlug(articleMsg);

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId(orderMsg));
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId(customerMsg));
    }
}
