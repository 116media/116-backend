using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Validator for the <see cref="AdminCreateOrderCommand" />.
/// </summary>
public class AdminCreateOrderValidator : AbstractValidator<AdminCreateOrderCommand>
{
    /// <summary>
    /// Configures validation rules for order creation.
    /// </summary>
    public AdminCreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).IsValidGuid("Customer ID");
    }
}
