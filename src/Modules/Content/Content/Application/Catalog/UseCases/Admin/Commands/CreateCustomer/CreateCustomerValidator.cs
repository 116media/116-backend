using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Validator for the <see cref="CreateCustomerCommand" /> ensuring proper customer data format.
/// </summary>
public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    /// <summary>
    /// Configures validation rules for customer creation.
    /// </summary>
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName).ValidCustomerFullName();
        RuleFor(x => x.Email).ValidCustomerEmail();
        RuleFor(x => x.Phone).ValidCustomerPhone();
        RuleFor(x => x.Company).ValidCustomerCompany();
        RuleFor(x => x.Notes).ValidCustomerNotes();
    }
}
