using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;

/// <summary>
/// Query for retrieving a paginated list of orders in PendingPayment status, ordered oldest-first.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record AdminGetPendingPaymentOrdersQuery(PaginatedRequest PaginatedRequest)
    : IQuery<AdminGetPendingPaymentOrdersResult>;

/// <summary>
/// Result of the <see cref="AdminGetPendingPaymentOrdersQuery" /> containing paginated order summaries.
/// </summary>
/// <param name="Orders">Paginated result containing order summary DTOs.</param>
public record AdminGetPendingPaymentOrdersResult(PaginatedResult<ContentOrderSummaryDto> Orders);
