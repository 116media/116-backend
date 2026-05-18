using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;

/// <summary>
/// Validator for the <see cref="AdminUpdateCustomerCommand" /> ensuring proper customer data format.
/// </summary>
public class AdminUpdateCustomerValidator : AbstractValidator<AdminUpdateCustomerCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateCustomerValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Customer validation error messages.</param>
    public AdminUpdateCustomerValidator(CustomerErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Customer ID");
        RuleFor(x => x.FullName).ValidCustomerFullName(msg);
        RuleFor(x => x.Email).ValidCustomerEmail(msg);
        RuleFor(x => x.Phone).ValidCustomerPhone(msg);
        RuleFor(x => x.Company).ValidCustomerCompany(msg);
        RuleFor(x => x.Notes).ValidCustomerNotes(msg);
    }
}
