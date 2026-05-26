using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Validator for the <see cref="AdminCreateOrderCommand" />.
/// </summary>
public class AdminCreateOrderValidator : AbstractValidator<AdminCreateOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateOrderValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Customer validation error messages.</param>
    public AdminCreateOrderValidator(CustomerErrorMessage i18n)
    {
        RuleFor(x => x.CustomerId).IsValidGuid(i18n.Localizer);
    }
}
