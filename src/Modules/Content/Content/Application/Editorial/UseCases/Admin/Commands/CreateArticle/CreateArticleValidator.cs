using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Validator for the <see cref="CreateArticleCommand" /> ensuring proper article draft creation data.
/// </summary>
public class CreateArticleValidator : AbstractValidator<CreateArticleCommand>
{
    /// <summary>
    /// Configures validation rules for article draft creation.
    /// </summary>
    public CreateArticleValidator()
    {
        RuleFor(x => x.CategoryId).ValidArticleCategoryId();

        RuleFor(x => x.Title).ValidArticleTitle();

        RuleFor(x => x.Slug).ValidArticleSlug();

        RuleFor(x => x.OrderItemId)
            .NotEmpty()
            .When(x => x.CustomerId.HasValue)
            .WithMessage("Order item ID is required when customer ID is provided.");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .When(x => x.OrderItemId.HasValue)
            .WithMessage("Customer ID is required when order item ID is provided.");
    }
}
