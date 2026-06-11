using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
        RuleFor(x => x.FullName)
            .ValidCustomerFullName(
                fullNameRequired: msg.FullNameRequired(),
                fullNameTooLong: msg.FullNameTooLong(ContentConstants.MaxCustomerFullNameLength)
            );
        RuleFor(x => x.Email)
            .ValidCustomerEmail(
                emailRequired: msg.EmailRequired(),
                emailInvalidFormat: msg.EmailInvalidFormat(),
                emailTooLong: msg.EmailTooLong(ContentConstants.MaxCustomerEmailLength)
            );
        RuleFor(x => x.Phone)
            .ValidCustomerPhone(phoneTooLong: msg.PhoneTooLong(ContentConstants.MaxCustomerPhoneLength));
        RuleFor(x => x.Company)
            .ValidCustomerCompany(companyTooLong: msg.CompanyTooLong(ContentConstants.MaxCustomerCompanyLength));
        RuleFor(x => x.Notes)
            .ValidCustomerNotes(notesTooLong: msg.NotesTooLong(ContentConstants.MaxCustomerNotesLength));
    }
}
