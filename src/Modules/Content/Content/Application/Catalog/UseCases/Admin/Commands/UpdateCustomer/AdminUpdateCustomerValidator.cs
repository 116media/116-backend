using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateCustomerValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Customer.Msg.Localizer);
        RuleFor(x => x.FullName).ValidCustomerFullName(i18n.Customer.Msg);
        RuleFor(x => x.Email).ValidCustomerEmail(i18n.Customer.Msg);
        RuleFor(x => x.Phone).ValidCustomerPhone(i18n.Customer.Msg);
        RuleFor(x => x.Company).ValidCustomerCompany(i18n.Customer.Msg);
        RuleFor(x => x.Notes).ValidCustomerNotes(i18n.Customer.Msg);
    }
}
