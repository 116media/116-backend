using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Validator for the <see cref="AdminAddOrderItemCommand" />.
/// </summary>
public class AdminAddOrderItemValidator : AbstractValidator<AdminAddOrderItemCommand>
{
    /// <summary>
    /// Configures validation rules for adding an order item.
    /// </summary>
    public AdminAddOrderItemValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
        RuleFor(x => x.ContentKind).ValidOrderItemContentKind();
    }
}
