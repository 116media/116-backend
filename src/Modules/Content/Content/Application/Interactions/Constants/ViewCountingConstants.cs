namespace _116.Content.Application.Interactions.Constants;

/// <summary>
/// Tuning knobs for short-video view counting: how long an identity's repeat views are
/// collapsed into one, and how long raw uncounted events are retained for fraud analysis.
/// </summary>
public static class ViewCountingConstants
{
    /// <summary>
    /// Window inside which repeat views from the same dedup key do not increment the
    /// displayed count. A same-day re-watch or refresh is one view.
    /// </summary>
    public static readonly TimeSpan DedupWindow = TimeSpan.FromHours(24);

    /// <summary>
    /// Retention for raw events that did not increment the count. Counted events are kept
    /// indefinitely as the auditable basis of the displayed number.
    /// </summary>
    public static readonly TimeSpan UncountedEventRetention = TimeSpan.FromDays(30);
}
