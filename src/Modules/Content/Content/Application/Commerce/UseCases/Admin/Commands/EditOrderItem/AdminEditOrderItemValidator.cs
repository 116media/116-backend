using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">Content order validation error messages.</param>
    /// <param name="categoryMsg">Category validation error messages.</param>
    public AdminEditOrderItemValidator(ContentOrderErrorMessage i18n, CategoryErrorMessage categoryMsg)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.ItemId).IsValidGuid(i18n.Localizer, "ItemIdRequired", "ItemIdInvalid");

        When(x => x.CategoryId is not null, () => RuleFor(x => x.CategoryId!).IsValidGuid(categoryMsg.Localizer));

        When(x => x.ContentKind.HasValue, () => RuleFor(x => x.ContentKind!.Value).ValidOrderItemContentKind(i18n));
    }
}
