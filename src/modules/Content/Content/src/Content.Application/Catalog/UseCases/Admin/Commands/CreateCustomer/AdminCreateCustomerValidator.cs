using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateCustomerValidator(ContentI18n i18n)
    {
        RuleFor(x => x.FullName).ValidCustomerFullName(i18n.Customer.Msg);
        RuleFor(x => x.Email).ValidCustomerEmail(i18n.Customer.Msg);
        RuleFor(x => x.Phone).ValidCustomerPhone(i18n.Customer.Msg);
        RuleFor(x => x.Company).ValidCustomerCompany(i18n.Customer.Msg);
        RuleFor(x => x.Notes).ValidCustomerNotes(i18n.Customer.Msg);
    }
}
