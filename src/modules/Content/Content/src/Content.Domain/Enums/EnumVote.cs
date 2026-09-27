namespace _116.Content.Domain.Enums;

/// <summary>
/// Represents a single user's vote on a pending revision. Shared between spec 10's
/// <c>LyricsTranslationVoteEntity</c> and spec 11's <c>LyricsRevisionVoteEntity</c>, which follow
/// the identical propose/vote/accept shape.
/// </summary>
public enum EnumVote
{
    /// <summary>
    /// The voter approves the proposed revision.
    /// </summary>
    Approve,

    /// <summary>
    /// The voter rejects the proposed revision.
    /// </summary>
    Reject,
}
