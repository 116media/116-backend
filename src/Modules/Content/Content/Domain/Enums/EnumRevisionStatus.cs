namespace _116.Content.Domain.Enums;

/// <summary>
/// Represents the review status of a proposed revision (a translation revision or a lyrics-text
/// revision). Shared between spec 10's <c>LyricsTranslationRevisionEntity</c> and spec 11's
/// <c>LyricsRevisionEntity</c>, which follow the identical propose/vote/accept shape.
/// </summary>
public enum EnumRevisionStatus
{
    /// <summary>
    /// The revision has been proposed and is awaiting either enough community votes to reach
    /// the auto-accept threshold, or a moderator override.
    /// </summary>
    Pending,

    /// <summary>
    /// The revision was accepted, either by crossing the community vote threshold or by a
    /// moderator override, and has been applied to the underlying published text.
    /// </summary>
    Accepted,

    /// <summary>
    /// The revision was rejected, either by the community vote tally or by a moderator override,
    /// and is never applied to the underlying published text.
    /// </summary>
    Rejected,
}
