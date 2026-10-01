using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Payment transitions of <see cref="ContentPaymentEntity" />. Its state lives in <c>Entities/ContentPaymentEntity.cs</c>.
/// </summary>
public partial class ContentPaymentEntity
{
    /// <summary>
    /// Records the customer's payment proof file and payment method.
    /// Called by <c>AttachPaymentProofHandler</c> after the admin uploads the receipt file.
    /// </summary>
    /// <param name="proofFileId">The ID of the file record stored in <c>storage.files</c>.</param>
    /// <param name="paymentMethod">The payment method used by the customer.</param>
    /// <exception cref="ContentRuleException">
    /// Thrown when the payment is no longer pending — proof must not be overwritten
    /// once a decision was made against it.
    /// </exception>
    public void AttachProof(Guid proofFileId, EnumPaymentMethod paymentMethod)
    {
        if (Status != EnumPaymentStatus.Pending)
        {
            throw new ContentRuleException(ContentRuleCodes.PaymentAlreadyDecided);
        }

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
    /// <exception cref="ContentRuleException">
    /// Thrown when the payment is already verified or rejected.
    /// </exception>
    public void Verify(Guid adminUserId, string receiptUrl, DateTimeOffset now)
    {
        if (Status == EnumPaymentStatus.Verified)
        {
            throw new ContentRuleException(ContentRuleCodes.PaymentAlreadyVerified);
        }

        if (Status == EnumPaymentStatus.Rejected)
        {
            throw new ContentRuleException(ContentRuleCodes.PaymentAlreadyRejected);
        }

        if (PaymentProofFileId is null || PaymentMethod is null)
        {
            throw new ContentRuleException(ContentRuleCodes.PaymentProofRequired);
        }

        Status = EnumPaymentStatus.Verified;
        VerifiedById = adminUserId;
        VerifiedAt = now;
        ReceiptUrl = receiptUrl;
    }

    /// <summary>
    /// Rejects the payment with optional notes explaining the reason.
    /// The order remains in <c>PendingPayment</c> so a corrected proof can be resubmitted.
    /// </summary>
    /// <param name="notes">Optional admin notes explaining why the proof was rejected.</param>
    /// <exception cref="ContentRuleException">
    /// Thrown when the payment is already verified or rejected.
    /// </exception>
    internal void Reject(string? notes)
    {
        if (Status == EnumPaymentStatus.Verified)
        {
            throw new ContentRuleException(ContentRuleCodes.PaymentAlreadyVerified);
        }

        if (Status == EnumPaymentStatus.Rejected)
        {
            throw new ContentRuleException(ContentRuleCodes.PaymentAlreadyRejected);
        }

        Status = EnumPaymentStatus.Rejected;
        Notes = notes;
    }
}
