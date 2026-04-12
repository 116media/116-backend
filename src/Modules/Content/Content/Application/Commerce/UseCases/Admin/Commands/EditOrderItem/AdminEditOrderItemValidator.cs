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
    /// Configures validation rules for editing an order item.
    /// </summary>
    public AdminEditOrderItemValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.ItemId).IsValidGuid("Item ID");

        When(x => x.CategoryId is not null, () => RuleFor(x => x.CategoryId!).IsValidGuid("Category ID"));

        When(x => x.ContentKind.HasValue, () => RuleFor(x => x.ContentKind!.Value).ValidOrderItemContentKind());
    }
}
