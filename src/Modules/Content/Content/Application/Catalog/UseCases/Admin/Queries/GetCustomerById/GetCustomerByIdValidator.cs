using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;

/// <summary>
/// Validator for the <see cref="GetCustomerByIdQuery" /> ensuring a valid customer ID is provided.
/// </summary>
public class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
{
    /// <summary>
    /// Configures validation rules for customer retrieval by ID.
    /// </summary>
    public GetCustomerByIdValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Customer ID");
    }
}
