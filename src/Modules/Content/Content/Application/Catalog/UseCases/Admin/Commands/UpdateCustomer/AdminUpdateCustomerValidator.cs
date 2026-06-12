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
    /// <param name="i18n">Customer validation error messages.</param>
    public AdminUpdateCustomerValidator(CustomerErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.FullName).ValidCustomerFullName(i18n);
        RuleFor(x => x.Email).ValidCustomerEmail(i18n);
        RuleFor(x => x.Phone).ValidCustomerPhone(i18n);
        RuleFor(x => x.Company).ValidCustomerCompany(i18n);
        RuleFor(x => x.Notes).ValidCustomerNotes(i18n);
    }
}
