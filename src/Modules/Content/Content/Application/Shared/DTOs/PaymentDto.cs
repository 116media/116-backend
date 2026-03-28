using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a content payment record.
/// </summary>
/// <param name="Id">The unique identifier of the payment record.</param>
/// <param name="AmountUsd">The total amount in USD the customer must pay.</param>
/// <param name="PaymentMethod">The payment method used by the customer, or null if not yet attached.</param>
/// <param name="PaymentProof">
/// The uploaded payment proof file, or null if not yet attached.
/// Includes the MIME type so the frontend can render an image or a PDF viewer accordingly.
/// </param>
/// <param name="Status">The current verification status of the payment.</param>
/// <param name="VerifiedBy">The identity user UUID of the admin who verified the payment, or null.</param>
/// <param name="VerifiedAt">When the payment was verified, or null.</param>
/// <param name="ReceiptUrl">The URL of the generated payment confirmation receipt, or null.</param>
public record PaymentDto(
    Guid Id,
    decimal AmountUsd,
    EnumPaymentMethod? PaymentMethod,
    FileDto? PaymentProof,
    EnumPaymentStatus Status,
    Guid? VerifiedBy,
    DateTimeOffset? VerifiedAt,
    string? ReceiptUrl
);
