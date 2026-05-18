using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Validator for the <see cref="AdminCreateCustomerCommand" /> ensuring proper customer data format.
/// </summary>
public class AdminCreateCustomerValidator : AbstractValidator<AdminCreateCustomerCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateCustomerValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Customer validation error messages.</param>
    public AdminCreateCustomerValidator(CustomerErrorMessage msg)
    {
        RuleFor(x => x.FullName).ValidCustomerFullName(msg);
        RuleFor(x => x.Email).ValidCustomerEmail(msg);
        RuleFor(x => x.Phone).ValidCustomerPhone(msg);
        RuleFor(x => x.Company).ValidCustomerCompany(msg);
        RuleFor(x => x.Notes).ValidCustomerNotes(msg);
    }
}
