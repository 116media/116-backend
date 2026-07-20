namespace _116.Content.Domain.Enums;

/// <summary>
/// Represents the moderation status of a community-submitted new song, prior to it becoming a
/// real published lyrics record.
/// </summary>
public enum EnumSubmissionStatus
{
    /// <summary>
    /// The submission has been created and is awaiting moderator review.
    /// </summary>
    Pending,

    /// <summary>
    /// The submission was approved and promoted into a real lyrics record.
    /// </summary>
    Approved,

    /// <summary>
    /// The submission was rejected outright and will not become a lyrics record.
    /// </summary>
    Rejected,

    /// <summary>
    /// The moderator asked the submitter to revise and resubmit the content.
    /// </summary>
    NeedsRevision,
}
