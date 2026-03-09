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
    /// Configures validation rules for customer update.
    /// </summary>
    public AdminUpdateCustomerValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Customer ID");
        RuleFor(x => x.FullName).ValidCustomerFullName();
        RuleFor(x => x.Phone).ValidCustomerPhone();
        RuleFor(x => x.Company).ValidCustomerCompany();
        RuleFor(x => x.Notes).ValidCustomerNotes();
    }
}
