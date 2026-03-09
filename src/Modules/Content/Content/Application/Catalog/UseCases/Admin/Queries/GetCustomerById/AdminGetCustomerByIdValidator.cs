using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;

/// <summary>
/// Validator for the <see cref="AdminGetCustomerByIdQuery" /> ensuring a valid customer ID is provided.
/// </summary>
public class AdminGetCustomerByIdValidator : AbstractValidator<AdminGetCustomerByIdQuery>
{
    /// <summary>
    /// Configures validation rules for customer retrieval by ID.
    /// </summary>
    public AdminGetCustomerByIdValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Customer ID");
    }
}
