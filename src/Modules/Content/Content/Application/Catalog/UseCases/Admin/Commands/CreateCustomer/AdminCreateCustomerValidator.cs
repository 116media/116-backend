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
    /// <param name="i18n">Customer validation error messages.</param>
    public AdminCreateCustomerValidator(CustomerErrorMessage i18n)
    {
        RuleFor(x => x.FullName).ValidCustomerFullName(i18n);
        RuleFor(x => x.Email).ValidCustomerEmail(i18n);
        RuleFor(x => x.Phone).ValidCustomerPhone(i18n);
        RuleFor(x => x.Company).ValidCustomerCompany(i18n);
        RuleFor(x => x.Notes).ValidCustomerNotes(i18n);
    }
}
