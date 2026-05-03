using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Validator for the <see cref="AdminEditOrderCommand" />.
/// </summary>
public class AdminEditOrderValidator : AbstractValidator<AdminEditOrderCommand>
{
    /// <summary>
    /// Configures validation rules for editing an order.
    /// </summary>
    public AdminEditOrderValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");

        When(x => x.CustomerId is not null, () => RuleFor(x => x.CustomerId!).IsValidGuid("Customer ID"));
    }
}
