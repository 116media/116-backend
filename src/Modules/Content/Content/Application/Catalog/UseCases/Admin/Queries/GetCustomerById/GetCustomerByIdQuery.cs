using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;

/// <summary>
/// Query for retrieving a single customer by its identifier.
/// </summary>
/// <param name="Id">The unique identifier of the customer.</param>
public record GetCustomerByIdQuery(string Id) : IQuery<GetCustomerByIdResult>;

/// <summary>
/// Result of the <see cref="GetCustomerByIdQuery" /> containing the customer details.
/// </summary>
/// <param name="Customer">The customer information.</param>
public record GetCustomerByIdResult(CustomerDto Customer);
