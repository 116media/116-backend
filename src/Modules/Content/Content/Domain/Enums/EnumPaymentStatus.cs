namespace _116.Content.Domain.Enums;

/// <summary>
/// Represents the verification status of a content payment record.
/// Stored as a <c>string</c> in the database via EF Core's <c>.HasConversion&lt;string&gt;()</c>.
/// </summary>
public enum EnumPaymentStatus
{
    /// <summary>
    /// The payment record has been created but no proof has been verified yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The payment proof has been reviewed and confirmed by an admin.
    /// Triggers social boost and featured stamps on linked content.
    /// </summary>
    Verified,

    /// <summary>
    /// The payment proof was rejected (invalid amount, unrecognised sender, etc.).
    /// The order remains in <c>PendingPayment</c> so a corrected proof can be resubmitted.
    /// </summary>
    Rejected,
}
