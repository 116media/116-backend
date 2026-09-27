using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Validator for the <see cref="AdminCreateVideoCommand" /> ensuring proper video draft creation data.
/// </summary>
public class AdminCreateVideoValidator : AbstractValidator<AdminCreateVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.CategoryId).ValidArticleCategoryId(i18n.Article.Msg.CategoryIdRequired());

        RuleFor(x => x.Title).ValidVideoTitle(i18n.Video.Msg);
        RuleFor(x => x.Slug).ValidVideoSlug(i18n.Video.Msg);

        RuleFor(x => x.Description).ValidVideoDescription(i18n.Video.Msg);

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId(i18n.ContentOrder.Msg));
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId(i18n.Customer.Msg));
    }
}
