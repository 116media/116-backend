using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders;

/// <summary>
/// Query for retrieving a paginated list of orders for a specific customer.
/// </summary>
/// <param name="CustomerId">The identifier of the customer.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record AdminGetCustomerOrdersQuery(string CustomerId, PaginatedRequest PaginatedRequest)
    : IQuery<AdminGetCustomerOrdersResult>;

/// <summary>
/// Result of the <see cref="AdminGetCustomerOrdersQuery" /> containing paginated order summaries.
/// </summary>
/// <param name="Orders">Paginated result containing order summary DTOs.</param>
public record AdminGetCustomerOrdersResult(PaginatedResult<ContentOrderSummaryDto> Orders);
