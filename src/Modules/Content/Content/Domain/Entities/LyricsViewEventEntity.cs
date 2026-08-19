using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Raw record of a single lyrics-page view event, kept separately from the cached
/// <c>ViewCount</c> so views can be deduplicated per identity and audited later.
/// Only events flagged <see cref="IsCounted" /> incremented the displayed count.
/// <para>
/// Carries <see cref="DwellMs" /> and <see cref="ScrollDepthRatio" /> alongside the
/// dedup/audit fields shared with other content types' view events — these two
/// columns feed the read-time view-counting algorithm that decides whether an
/// event is genuinely a completed read before it is flagged <see cref="IsCounted" />.
/// </para>
/// </summary>
public class LyricsViewEventEntity : Aggregate<Guid>
{
    /// <summary>
    /// The lyrics page that was viewed.
    /// </summary>
    public Guid LyricsId { get; private set; }

    /// <summary>
    /// The identity user UUID of the viewer. Null for anonymous views.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// The identity surrogate the view is deduplicated against, in priority order:
    /// <c>user:{userId}</c>, else <c>device:{X-Device-Id}</c>, else <c>ip:{address}</c>,
    /// else <c>unknown</c>.
    /// </summary>
    public string DedupKey { get; private set; } = string.Empty;

    /// <summary>
    /// The caller's IP address, kept as a secondary fraud signal only. Null when unresolvable.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// The caller's User-Agent header, kept as a secondary fraud signal only. Null when absent.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Whether this event incremented the displayed view count. False when another counted
    /// event with the same dedup key already exists inside the dedup window, or when the
    /// read-time view-counting algorithm rejects the event as not a genuine read.
    /// </summary>
    public bool IsCounted { get; private set; }

    /// <summary>
    /// Total foreground dwell time on the lyrics page, in milliseconds, as reported by the
    /// client. Consumed by the read-time view-counting algorithm to gate whether a view
    /// counts, alongside <see cref="ScrollDepthRatio" />.
    /// </summary>
    public int DwellMs { get; private set; }

    /// <summary>
    /// The maximum scroll coverage reached while viewing the lyrics text, expressed as a
    /// ratio between 0.0 (no scroll) and 1.0 (fully scrolled to the end).
    /// </summary>
    public double ScrollDepthRatio { get; private set; }

    /// <summary>
    /// Navigation property to the lyrics page.
    /// </summary>
    public LyricsEntity Lyrics { get; private set; } = null!;

    private LyricsViewEventEntity() { }

    /// <summary>
    /// Creates a new raw lyrics-page view event.
    /// </summary>
    /// <param name="id">The unique identifier for this event.</param>
    /// <param name="lyricsId">The lyrics page that was viewed.</param>
    /// <param name="userId">The viewer's identity user UUID. Null for anonymous views.</param>
    /// <param name="dedupKey">The identity surrogate used for deduplication.</param>
    /// <param name="ipAddress">The caller's IP address, or null.</param>
    /// <param name="userAgent">The caller's User-Agent header, or null.</param>
    /// <param name="isCounted">Whether this event incremented the displayed view count.</param>
    /// <param name="dwellMs">Total foreground dwell time on the page, in milliseconds.</param>
    /// <param name="scrollDepthRatio">Maximum scroll coverage reached, from 0.0 to 1.0.</param>
    /// <returns>A new <see cref="LyricsViewEventEntity" />.</returns>
    public static LyricsViewEventEntity Create(
        Guid id,
        Guid lyricsId,
        Guid? userId,
        string dedupKey,
        string? ipAddress,
        string? userAgent,
        bool isCounted,
        int dwellMs,
        double scrollDepthRatio
    )
    {
        var viewEvent = new LyricsViewEventEntity
        {
            Id = id,
            LyricsId = lyricsId,
            UserId = userId,
            DedupKey = dedupKey,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsCounted = isCounted,
            DwellMs = dwellMs,
            ScrollDepthRatio = scrollDepthRatio,
            CreatedAt = DateTime.UtcNow,
        };

        if (isCounted)
        {
            viewEvent.AddDomainEvent(
                new LyricsEngagedEvent(LyricsId: lyricsId, Kind: EnumEngagementKind.View, Delta: 1)
            );
        }

        return viewEvent;
    }
}
