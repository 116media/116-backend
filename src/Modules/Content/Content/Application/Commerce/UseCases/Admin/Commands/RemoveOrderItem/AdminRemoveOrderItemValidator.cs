using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;

/// <summary>
/// Validator for the <see cref="AdminRemoveOrderItemCommand" />.
/// </summary>
public class AdminRemoveOrderItemValidator : AbstractValidator<AdminRemoveOrderItemCommand>
{
    /// <summary>
    /// Configures validation rules for removing an order item.
    /// </summary>
    public AdminRemoveOrderItemValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.ItemId).IsValidGuid("Item ID");
    }
}
