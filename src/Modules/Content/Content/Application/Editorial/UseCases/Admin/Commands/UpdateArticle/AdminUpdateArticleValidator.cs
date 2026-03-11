using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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

        RuleFor(x => x.OrderItemId)
            .NotEmpty()
            .When(x => x.CustomerId.HasValue)
            .WithMessage("Order item ID is required when customer ID is provided.");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .When(x => x.OrderItemId.HasValue)
            .WithMessage("Customer ID is required when order item ID is provided.");

        RuleFor(x => x.FeaturedUntil)
            .GreaterThan(DateTimeOffset.UtcNow)
            .When(x => x.FeaturedUntil.HasValue)
            .WithMessage("Featured until date must be in the future.");

        RuleFor(x => x.MetaTitle)
            .MinimumLength(ContentConstants.MinMetaTitleLength)
            .MaximumLength(ContentConstants.MaxMetaTitleLength)
            .When(x => x.MetaTitle is not null);

        RuleFor(x => x.MetaDescription)
            .MinimumLength(ContentConstants.MinMetaDescriptionLength)
            .MaximumLength(ContentConstants.MaxMetaDescriptionLength)
            .When(x => x.MetaDescription is not null);
    }
}
