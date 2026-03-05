using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;

/// <summary>
/// Query for retrieving a paginated list of B2B customers, ordered by most recently created first.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record GetAllCustomersQuery(PaginatedRequest PaginatedRequest) : IQuery<GetAllCustomersResult>;

/// <summary>
/// Result of the <see cref="GetAllCustomersQuery" /> containing paginated customer DTOs.
/// </summary>
/// <param name="Customers">Paginated result containing customer DTOs.</param>
public record GetAllCustomersResult(PaginatedResult<CustomerDto> Customers);
