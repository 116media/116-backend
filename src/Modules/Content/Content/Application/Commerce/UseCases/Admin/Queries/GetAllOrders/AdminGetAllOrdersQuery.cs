using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;

/// <summary>
/// Query for retrieving a paginated list of orders with optional status and customer filters.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="Status">Optional filter by order status.</param>
/// <param name="CustomerId">Optional filter by customer identifier.</param>
public record AdminGetAllOrdersQuery(PaginatedRequest PaginatedRequest, EnumOrderStatus? Status, Guid? CustomerId)
    : IQuery<AdminGetAllOrdersResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllOrdersQuery" /> containing paginated order summaries.
/// </summary>
/// <param name="Orders">Paginated result containing order summary DTOs.</param>
public record AdminGetAllOrdersResult(PaginatedResult<ContentOrderSummaryDto> Orders);
