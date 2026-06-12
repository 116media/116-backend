using _116.Content.Application.Shared.Errors.Facade;
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
    /// Initializes a new instance of <see cref="AdminUpdateArticleValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateArticleValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Article.Msg.Localizer);

        RuleFor(x => x.CategoryId).ValidArticleCategoryId(i18n.Article.Msg.CategoryIdRequired());

        RuleFor(x => x.Title).ValidArticleTitle(i18n.Article.Msg);

        RuleFor(x => x.Slug).ValidArticleSlug(i18n.Article.Msg);

        RuleFor(x => x.Headline).ValidArticleHeadline(i18n.Article.Msg);

        RuleFor(x => x.Body).ValidArticleBody(i18n.Article.Msg);

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId(i18n.ContentOrder.Msg));
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId(i18n.Customer.Msg));

        When(
            x => x.MetaTitle is not null,
            () =>
                RuleFor(x => x.MetaTitle)
                    .ValidMetaTitle(
                        metaTitleTooShort: i18n.Article.Msg.MetaTitleTooShort(ContentConstants.MinMetaTitleLength),
                        metaTitleTooLong: i18n.Article.Msg.MetaTitleTooLong(ContentConstants.MaxMetaTitleLength)
                    )
        );
        When(
            x => x.MetaDescription is not null,
            () =>
                RuleFor(x => x.MetaDescription)
                    .ValidMetaDescription(
                        metaDescriptionTooShort: i18n.Article.Msg.MetaDescriptionTooShort(
                            ContentConstants.MinMetaDescriptionLength
                        ),
                        metaDescriptionTooLong: i18n.Article.Msg.MetaDescriptionTooLong(
                            ContentConstants.MaxMetaDescriptionLength
                        )
                    )
        );
    }
}
