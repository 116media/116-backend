using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Validator for the <see cref="AdminEditOrderCommand" />.
/// </summary>
public class AdminEditOrderValidator : AbstractValidator<AdminEditOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminEditOrderValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="orderMsg">Content order validation error messages.</param>
    /// <param name="customerMsg">Customer validation error messages.</param>
    public AdminEditOrderValidator(ContentOrderErrorMessage orderMsg, CustomerErrorMessage customerMsg)
    {
        RuleFor(x => x.OrderId).IsValidGuid(orderMsg.Localizer);

        When(x => x.CustomerId is not null, () => RuleFor(x => x.CustomerId!).IsValidGuid(customerMsg.Localizer));
    }
}
