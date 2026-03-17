using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents the payment record for a <see cref="ContentOrderEntity" />.
/// Created when the order is submitted and tracks the full payment lifecycle:
/// proof attachment → admin verification (or rejection).
/// </summary>
public class ContentPaymentEntity : Aggregate<Guid>
{
    /// <summary>
    /// The order this payment record belongs to.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// The total amount in USD that the customer must pay, snapshotted from the order total at submission time.
    /// </summary>
    public decimal AmountUsd { get; private set; }

    /// <summary>
    /// The payment method used by the customer (e.g., BankTransfer, MobileMoney, Cash).
    /// Set when the admin attaches the payment proof.
    /// </summary>
    public EnumPaymentMethod? PaymentMethod { get; private set; }

    /// <summary>
    /// The ID of the file record in <c>core.files</c> for the uploaded payment proof.
    /// Set when the admin attaches the proof via <see cref="AttachProof" />.
    /// </summary>
    public Guid? PaymentProofFileId { get; private set; }

    /// <summary>
    /// Current verification status of the payment.
    /// </summary>
    public EnumPaymentStatus Status { get; private set; }

    /// <summary>
    /// The identity user UUID of the admin who verified this payment.
    /// <c>null</c> until <see cref="Verify" /> is called.
    /// </summary>
    public Guid? VerifiedById { get; private set; }

    /// <summary>
    /// When the payment was verified by the admin.
    /// <c>null</c> until <see cref="Verify" /> is called.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; private set; }

    /// <summary>
    /// URL of the downloadable payment receipt generated after verification.
    /// <c>null</c> until <see cref="Verify" /> is called.
    /// </summary>
    public string? ReceiptUrl { get; private set; }

    /// <summary>
    /// Optional notes from the admin when rejecting the payment.
    /// Explains why the proof was rejected so the team can follow up with the client.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// The order this payment belongs to.
    /// </summary>
    public ContentOrderEntity Order { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ContentPaymentEntity() { }

    /// <summary>
    /// Creates a new payment record for an order at the moment of submission.
    /// The payment starts in <c>Pending</c> status with no proof attached.
    /// </summary>
    /// <param name="id">The unique identifier for the payment record.</param>
    /// <param name="orderId">The order this payment belongs to.</param>
    /// <param name="amountUsd">The total amount the customer must pay (snapshotted from the order).</param>
    /// <returns>A new <see cref="ContentPaymentEntity" /> in <c>Pending</c> status.</returns>
    public static ContentPaymentEntity Create(Guid id, Guid orderId, decimal amountUsd)
    {
        return new ContentPaymentEntity
        {
            Id = id,
            OrderId = orderId,
            AmountUsd = amountUsd,
            Status = EnumPaymentStatus.Pending,
        };
    }

    /// <summary>
    /// Records the customer's payment proof file and payment method.
    /// Called by <c>AttachPaymentProofHandler</c> after the admin uploads the receipt file.
    /// </summary>
    /// <param name="proofFileId">The ID of the file record stored in <c>core.files</c>.</param>
    /// <param name="paymentMethod">The payment method used by the customer.</param>
    public void AttachProof(Guid proofFileId, EnumPaymentMethod paymentMethod)
    {
        PaymentProofFileId = proofFileId;
        PaymentMethod = paymentMethod;
    }

    /// <summary>
    /// Verifies the payment, transitioning status to <c>Verified</c>.
    /// Records who verified it and when, and stores the receipt URL.
    /// Called by <c>VerifyPaymentHandler</c> only.
    /// </summary>
    /// <param name="adminUserId">The identity user UUID of the admin performing the verification.</param>
    /// <param name="receiptUrl">The URL of the generated payment confirmation receipt.</param>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown when the payment is already verified or rejected.
    /// </exception>
    public void Verify(Guid adminUserId, string receiptUrl)
    {
        if (Status == EnumPaymentStatus.Verified)
        {
            throw ContentOrderErrors.PaymentAlreadyVerified();
        }

        if (Status == EnumPaymentStatus.Rejected)
        {
            throw ContentOrderErrors.PaymentAlreadyRejected();
        }

        Status = EnumPaymentStatus.Verified;
        VerifiedById = adminUserId;
        VerifiedAt = DateTimeOffset.UtcNow;
        ReceiptUrl = receiptUrl;
    }

    /// <summary>
    /// Rejects the payment with optional notes explaining the reason.
    /// The order remains in <c>PendingPayment</c> so a corrected proof can be resubmitted.
    /// </summary>
    /// <param name="notes">Optional admin notes explaining why the proof was rejected.</param>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown when the payment is already verified or rejected.
    /// </exception>
    public void Reject(string? notes)
    {
        if (Status == EnumPaymentStatus.Verified)
        {
            throw ContentOrderErrors.PaymentAlreadyVerified();
        }

        if (Status == EnumPaymentStatus.Rejected)
        {
            throw ContentOrderErrors.PaymentAlreadyRejected();
        }

        Status = EnumPaymentStatus.Rejected;
        Notes = notes;
    }
}
