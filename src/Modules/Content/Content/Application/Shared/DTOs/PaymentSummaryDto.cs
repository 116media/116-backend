using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a payment record summary — used in the payments list view.
/// Includes the linked order and customer information so the frontend can display
/// a complete payment row without additional API calls.
/// </summary>
/// <param name="Id">The unique identifier of the payment record.</param>
/// <param name="OrderId">The order this payment belongs to.</param>
/// <param name="CustomerName">The full name of the B2B customer who placed the order.</param>
/// <param name="AmountUsd">The total amount in USD the customer must pay.</param>
/// <param name="PaymentMethod">The payment method used, or null if not yet attached.</param>
/// <param name="Status">The current verification status of the payment.</param>
/// <param name="OrderStatus">The lifecycle status of the linked order.</param>
/// <param name="VerifiedBy">The identity user UUID of the admin who verified, or null.</param>
/// <param name="VerifiedAt">When the payment was verified, or null.</param>
public record PaymentSummaryDto(
    Guid Id,
    Guid OrderId,
    string CustomerName,
    decimal AmountUsd,
    EnumPaymentMethod? PaymentMethod,
    EnumPaymentStatus Status,
    EnumOrderStatus OrderStatus,
    Guid? VerifiedBy,
    DateTimeOffset? VerifiedAt
) : AuditableDto;
