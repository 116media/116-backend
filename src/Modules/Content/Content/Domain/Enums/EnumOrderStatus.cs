namespace _116.Content.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a content order in the B2B revenue flow.
/// Stored as a <c>string</c> in the database via EF Core's <c>.HasConversion&lt;string&gt;()</c>.
/// </summary>
public enum EnumOrderStatus
{
    /// <summary>
    /// The order has been created but not yet submitted. Items and tiers can still be added.
    /// </summary>
    Draft,

    /// <summary>
    /// The order has been submitted and is awaiting customer payment.
    /// No new items or tiers can be added after this point.
    /// </summary>
    PendingPayment,

    /// <summary>
    /// Payment has been verified. Content creation can now begin.
    /// Social boost and featured stamps are applied to linked articles and videos.
    /// </summary>
    Paid,

    /// <summary>
    /// The order was cancelled before payment was completed.
    /// Only allowed from <c>Draft</c> or <c>PendingPayment</c> status.
    /// </summary>
    Cancelled,
}
