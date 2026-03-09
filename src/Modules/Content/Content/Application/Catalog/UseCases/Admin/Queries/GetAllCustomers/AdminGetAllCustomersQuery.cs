using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;

/// <summary>
/// Query for retrieving a paginated list of B2B customers, ordered by most recently created first.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record AdminGetAllCustomersQuery(PaginatedRequest PaginatedRequest) : IQuery<AdminGetAllCustomersResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllCustomersQuery" /> containing paginated customer DTOs.
/// </summary>
/// <param name="Customers">Paginated result containing customer DTOs.</param>
public record AdminGetAllCustomersResult(PaginatedResult<CustomerDto> Customers);
