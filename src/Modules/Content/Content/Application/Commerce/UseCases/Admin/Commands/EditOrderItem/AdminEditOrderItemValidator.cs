using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Validator for the <see cref="AdminEditOrderItemCommand" />.
/// </summary>
public class AdminEditOrderItemValidator : AbstractValidator<AdminEditOrderItemCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminEditOrderItemValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminEditOrderItemValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);
        RuleFor(x => x.ItemId).IsValidGuid(i18n.ContentOrder.Msg.Localizer, "ItemIdRequired", "ItemIdInvalid");

        When(x => x.CategoryId is not null, () => RuleFor(x => x.CategoryId!).IsValidGuid(i18n.Category.Msg.Localizer));

        When(
            x => x.ContentKind.HasValue,
            () => RuleFor(x => x.ContentKind!.Value).ValidOrderItemContentKind(i18n.ContentOrder.Msg)
        );
    }
}
