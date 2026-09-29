namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Limit bounds for the popular-videos endpoint. Shared by the endpoint (which clamps the
/// raw query-string value) and the query validator (which guards the dispatched query), so
/// the two never drift apart.
/// </summary>
public static class PopularVideosLimits
{
    /// <summary>
    /// Default number of videos returned when the caller does not supply a limit.
    /// Sized for the video-detail popular sidebar.
    /// </summary>
    public const int DefaultLimit = 10;

    /// <summary>
    /// Smallest accepted limit value.
    /// </summary>
    public const int MinLimit = 1;

    /// <summary>
    /// Largest accepted limit value. Caps the ranking query's row count so a caller cannot
    /// request an unbounded result set.
    /// </summary>
    public const int MaxLimit = 50;
}
