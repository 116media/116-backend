using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Validator for the <see cref="AdminCreateVideoCommand" /> ensuring proper video draft creation data.
/// </summary>
public class AdminCreateVideoValidator : AbstractValidator<AdminCreateVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateVideoValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="articleMsg">Article validation error messages.</param>
    /// <param name="videoMsg">Video validation error messages.</param>
    /// <param name="orderMsg">Content order validation error messages.</param>
    /// <param name="customerMsg">Customer validation error messages.</param>
    public AdminCreateVideoValidator(
        ArticleErrorMessage articleMsg,
        VideoErrorMessage videoMsg,
        ContentOrderErrorMessage orderMsg,
        CustomerErrorMessage customerMsg
    )
    {
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
    }
}
