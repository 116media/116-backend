namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Tuning knobs for the lyrics translation community review workflow (spec 10): the propose/
/// vote/threshold-accept mechanics for a <c>LyricsTranslationRevisionEntity</c>. Kept as a
/// small, named constants class rather than an inline literal in the handler, matching this
/// module's convention for other per-feature tunable thresholds.
/// </summary>
public static class TranslationConstants
{
    /// <summary>
    /// The net approval count (approvals minus rejections) a pending translation revision must
    /// reach for the community vote to auto-accept it, without a moderator override.
    /// </summary>
    public const int AutoAcceptThreshold = 3;
}
