namespace _116.Content.Domain.Enums;

/// <summary>
/// Represents the lifecycle status for a piece of content (article or video).
/// Stored as a <c>string</c> in the database via EF Core's <c>.HasConversion&lt;string&gt;()</c>.
/// </summary>
public enum EnumContentStatus
{
    /// <summary>
    /// The content has been created but not yet submitted for review or payment.
    /// Draft articles have empty <c>body</c> and <c>headline</c> fields.
    /// </summary>
    Draft,

    /// <summary>
    /// The content has been submitted and is awaiting customer payment.
    /// Only applies to paid content (content linked to an order item).
    /// </summary>
    PendingPayment,

    /// <summary>
    /// Payment has been verified (or content is free), and the editorial team
    /// is reviewing the content before approval.
    /// </summary>
    PendingReview,

    /// <summary>
    /// The editorial team has approved the content. It is cleared for publication.
    /// For videos, a YouTube link must still be attached before publishing.
    /// </summary>
    Approved,

    /// <summary>
    /// The content is live and visible to all public visitors.
    /// </summary>
    Published,

    /// <summary>
    /// The content was rejected during editorial review. The admin can revise
    /// and resubmit the content.
    /// </summary>
    Rejected,

    /// <summary>
    /// The content has been removed from all public feeds but not permanently deleted.
    /// Archiving is reversible — Cloudinary assets are not deleted on archive.
    /// </summary>
    Archived,
}
