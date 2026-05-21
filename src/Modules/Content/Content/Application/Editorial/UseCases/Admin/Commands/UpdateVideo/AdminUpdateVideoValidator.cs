using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Validator for the <see cref="AdminUpdateVideoCommand" /> ensuring all editable video fields are valid.
/// </summary>
public class AdminUpdateVideoValidator : AbstractValidator<AdminUpdateVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateVideoValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="articleMsg">Article validation error messages.</param>
    /// <param name="videoMsg">Video validation error messages.</param>
    /// <param name="orderMsg">Content order validation error messages.</param>
    /// <param name="customerMsg">Customer validation error messages.</param>
    public AdminUpdateVideoValidator(
        ArticleErrorMessage articleMsg,
        VideoErrorMessage videoMsg,
        ContentOrderErrorMessage orderMsg,
        CustomerErrorMessage customerMsg
    )
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");

        RuleFor(x => x.CategoryId).ValidArticleCategoryId(articleMsg.CategoryIdRequired());

        RuleFor(x => x.Title)
            .ValidVideoTitle(
                titleRequired: videoMsg.TitleRequired(),
                titleTooLong: videoMsg.TitleTooLong(ContentConstants.MaxTitleLength)
            );

        RuleFor(x => x.Slug)
            .ValidVideoSlug(
                slugRequired: videoMsg.SlugRequired(),
                slugTooLong: videoMsg.SlugTooLong(ContentConstants.MaxSlugLength),
                slugInvalidFormat: videoMsg.SlugInvalidFormat()
            );

        RuleFor(x => x.Description).ValidVideoDescription(descriptionRequired: videoMsg.DescriptionRequired());

        When(
            x => x.CustomerId.HasValue,
            () => RuleFor(x => x.OrderItemId).ValidOrderItemId(orderMsg.OrderItemIdRequired())
        );
        When(
            x => x.OrderItemId.HasValue,
            () => RuleFor(x => x.CustomerId).ValidCustomerId(customerMsg.CustomerIdRequired())
        );

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
