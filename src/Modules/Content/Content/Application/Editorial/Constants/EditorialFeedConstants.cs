namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Backend-internal constants governing homepage promotion feed data fetching.
/// Strip sizes are intentionally excluded — those are controlled by the frontend
/// via the <c>stripSize</c> query parameter (default: 3).
/// </summary>
public static class EditorialFeedConstants
{
    /// <summary>
    /// Maximum number of gossip articles fetched as the fallback pool for the
    /// article promotion feed. Items from this pool fill empty spots and the
    /// gossip strip.
    /// </summary>
    public const int GossipPoolSize = 20;

    /// <summary>
    /// Maximum number of free videos fetched as the fallback pool for the video
    /// promotion feed. Items from this pool fill empty spots and the free-video
    /// strip.
    /// </summary>
    public const int FreeVideoPoolSize = 20;

    /// <summary>
    /// Default number of items returned in the strip when the caller does not
    /// supply a <c>stripSize</c> query parameter.
    /// </summary>
    public const int DefaultStripSize = 3;

    /// <summary>
    /// Spot 1 — Hero carousel (top-left position in the homepage grid).
    /// </summary>
    public const int Spot1 = 1;

    /// <summary>
    /// Spot 2 — Side carousel (top-right position in the homepage grid).
    /// </summary>
    public const int Spot2 = 2;

    /// <summary>
    /// Spot 3 — Small pair (bottom-left position in the homepage grid), split into two columns.
    /// </summary>
    public const int Spot3 = 3;
}
