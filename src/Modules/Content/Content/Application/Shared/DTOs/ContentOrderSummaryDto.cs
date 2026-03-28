using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a content order summary — used in paginated list views.
/// </summary>
/// <param name="Id">The unique identifier of the order.</param>
/// <param name="CustomerName">The full name of the B2B customer who placed the order.</param>
/// <param name="Status">The current lifecycle status of the order.</param>
/// <param name="TotalAmountUsd">The running total of all tier and promotion price snapshots in USD.</param>
/// <param name="ItemCount">The number of commissioned content items in this order.</param>
public record ContentOrderSummaryDto(
    Guid Id,
    string CustomerName,
    EnumOrderStatus Status,
    decimal TotalAmountUsd,
    int ItemCount
) : AuditableDto;
