namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a content order detail view.
/// Extends the summary with the full list of commissioned items and the payment record.
/// </summary>
/// <param name="Id">The unique identifier of the order.</param>
/// <param name="CustomerName">The full name of the B2B customer who placed the order.</param>
/// <param name="Status">The current lifecycle status of the order.</param>
/// <param name="TotalAmountUsd">The running total of all tier and promotion price snapshots in USD.</param>
/// <param name="CreatedAt">When the order was created.</param>
/// <param name="Items">The commissioned content items in this order.</param>
/// <param name="Payment">The payment record, or null while the order is still in Draft status.</param>
public record ContentOrderDetailDto(
    Guid Id,
    string CustomerName,
    string Status,
    decimal TotalAmountUsd,
    DateTime? CreatedAt,
    IReadOnlyList<OrderItemDto> Items,
    PaymentDto? Payment
);
