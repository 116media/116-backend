using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminAddOrderItemValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Content order validation error messages.</param>
    public AdminAddOrderItemValidator(ContentOrderErrorMessage msg)
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.CategoryId).IsValidGuid("Category ID");
        RuleFor(x => x.ContentKind).ValidOrderItemContentKind(msg.InvalidOrderItemContentKind());
    }
}
