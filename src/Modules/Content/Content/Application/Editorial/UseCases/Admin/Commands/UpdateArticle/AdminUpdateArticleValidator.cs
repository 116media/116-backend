using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminUpdateArticleValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="articleMsg">Article validation error messages.</param>
    /// <param name="orderMsg">Content order validation error messages.</param>
    /// <param name="customerMsg">Customer validation error messages.</param>
    public AdminUpdateArticleValidator(
        ArticleErrorMessage articleMsg,
        ContentOrderErrorMessage orderMsg,
        CustomerErrorMessage customerMsg
    )
    {
        RuleFor(x => x.Id).IsValidGuid(articleMsg.Localizer);

        RuleFor(x => x.CategoryId).ValidArticleCategoryId(articleMsg.CategoryIdRequired());

        RuleFor(x => x.Title).ValidArticleTitle(articleMsg);

        RuleFor(x => x.Slug).ValidArticleSlug(articleMsg);

        RuleFor(x => x.Headline).ValidArticleHeadline(articleMsg);

        RuleFor(x => x.Body).ValidArticleBody(articleMsg);

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId(orderMsg));
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId(customerMsg));

        When(
            x => x.MetaTitle is not null,
            () =>
                RuleFor(x => x.MetaTitle)
                    .ValidMetaTitle(
                        metaTitleTooShort: articleMsg.MetaTitleTooShort(ContentConstants.MinMetaTitleLength),
                        metaTitleTooLong: articleMsg.MetaTitleTooLong(ContentConstants.MaxMetaTitleLength)
                    )
        );
        When(
            x => x.MetaDescription is not null,
            () =>
                RuleFor(x => x.MetaDescription)
                    .ValidMetaDescription(
                        metaDescriptionTooShort: articleMsg.MetaDescriptionTooShort(
                            ContentConstants.MinMetaDescriptionLength
                        ),
                        metaDescriptionTooLong: articleMsg.MetaDescriptionTooLong(
                            ContentConstants.MaxMetaDescriptionLength
                        )
                    )
        );
    }
}
