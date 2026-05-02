using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;

/// <summary>
/// Query for retrieving a paginated list of payment records with optional filters.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="Status">Optional filter by payment verification status.</param>
/// <param name="Method">Optional filter by payment method.</param>
/// <param name="Search">Optional search term matching customer name, email, or company.</param>
public record AdminGetAllPaymentsQuery(
    PaginatedRequest PaginatedRequest,
    EnumPaymentStatus? Status,
    EnumPaymentMethod? Method,
    string? Search = null
) : IQuery<AdminGetAllPaymentsResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllPaymentsQuery" /> containing paginated payment summaries.
/// </summary>
/// <param name="Payments">Paginated result containing payment summary DTOs.</param>
public record AdminGetAllPaymentsResult(PaginatedResult<PaymentSummaryDto> Payments);
