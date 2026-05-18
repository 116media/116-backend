using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
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

        RuleFor(x => x.CategoryId).ValidArticleCategoryId(articleMsg);

        RuleFor(x => x.Title).ValidVideoTitle(videoMsg);

        RuleFor(x => x.Slug).ValidVideoSlug(videoMsg);

        RuleFor(x => x.Description).ValidVideoDescription(videoMsg);

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId(orderMsg));
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId(customerMsg));

        When(x => x.MetaTitle is not null, () => RuleFor(x => x.MetaTitle).ValidMetaTitle(articleMsg));
        When(
            x => x.MetaDescription is not null,
            () => RuleFor(x => x.MetaDescription).ValidMetaDescription(articleMsg)
        );
    }
}
