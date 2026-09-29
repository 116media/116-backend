namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Tuning knobs for the lyrics-text community correction workflow (spec 11): the propose/
/// vote/threshold-accept mechanics for a <c>LyricsRevisionEntity</c>. Kept as a separate,
/// lyrics-scoped sibling of <see cref="TranslationConstants" /> rather than reusing that
/// translation-named class from lyrics correction code, even though both currently share the
/// same numeric value — a future change to one workflow's threshold should not silently
/// change the other's.
/// </summary>
public static class LyricsRevisionConstants
{
    /// <summary>
    /// The net approval count (approvals minus rejections) a pending lyrics-text revision must
    /// reach for the community vote to auto-accept it, without a moderator override.
    /// </summary>
    public const int AutoAcceptThreshold = 3;
}
