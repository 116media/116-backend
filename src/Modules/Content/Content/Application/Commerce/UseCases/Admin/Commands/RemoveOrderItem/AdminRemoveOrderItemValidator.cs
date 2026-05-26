using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;

/// <summary>
/// Validator for the <see cref="AdminRemoveOrderItemCommand" />.
/// </summary>
public class AdminRemoveOrderItemValidator : AbstractValidator<AdminRemoveOrderItemCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRemoveOrderItemValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content order validation error messages.</param>
    public AdminRemoveOrderItemValidator(ContentOrderErrorMessage i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.ItemId).IsValidGuid(i18n.Localizer, "ItemIdRequired", "ItemIdInvalid");
    }
}
