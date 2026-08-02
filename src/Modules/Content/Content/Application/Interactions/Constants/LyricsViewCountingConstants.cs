namespace _116.Content.Application.Interactions.Constants;

/// <summary>
/// Tuning knobs for the lyrics read-time view-counting algorithm (spec 05): whether a reported
/// dwell time and scroll depth represent a genuine completed read of the lyrics text, before a
/// view is allowed to increment the cached count. Kept as a sibling class to
/// <see cref="ViewCountingConstants" /> rather than folded into it — these knobs tune the
/// read-time gate specific to lyrics pages, not the shared dedup-window/retention behaviour.
/// </summary>
public static class LyricsViewCountingConstants
{
    /// <summary>
    /// Assumed reading speed in words per minute, used to derive the expected reading time
    /// for a lyrics page from its own word count.
    /// </summary>
    public const double WordsPerMinute = 130.0;

    /// <summary>
    /// The minimum fraction of the expected reading time a reported dwell time must reach
    /// for a view to count as a genuine read.
    /// </summary>
    public const double MinReadTimeRatio = 0.6;

    /// <summary>
    /// Upper bound on the dwell time required to count a view, regardless of how long the
    /// expected reading time computes to for very long lyrics pages.
    /// </summary>
    public static readonly TimeSpan MaxRequiredDwell = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Absolute minimum dwell time below which a view never counts, regardless of how short
    /// the expected reading time computes to for very short lyrics pages.
    /// </summary>
    public static readonly TimeSpan MinDwellFloor = TimeSpan.FromSeconds(1.5);

    /// <summary>
    /// The minimum scroll depth ratio a viewer must reach for a view to count as a genuine
    /// read, regardless of dwell time.
    /// </summary>
    public const double MinScrollDepthRatio = 0.7;
}
