using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Raw record of a single short-video view event, kept separately from the cached
/// <c>ViewCount</c> so views can be deduplicated per identity and audited later.
/// Only events flagged <see cref="IsCounted" /> incremented the displayed count.
/// </summary>
public class ShortVideoViewEventEntity : Aggregate<Guid>
{
    /// <summary>
    /// The short video that was viewed.
    /// </summary>
    public Guid ShortVideoId { get; private set; }

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
    /// event with the same dedup key already exists inside the dedup window.
    /// </summary>
    public bool IsCounted { get; private set; }

    /// <summary>
    /// Navigation property to the short video.
    /// </summary>
    public ShortVideoEntity ShortVideo { get; private set; } = null!;

    private ShortVideoViewEventEntity() { }

    /// <summary>
    /// Creates a new raw short-video view event.
    /// </summary>
    /// <param name="id">The unique identifier for this event.</param>
    /// <param name="shortVideoId">The short video that was viewed.</param>
    /// <param name="userId">The viewer's identity user UUID. Null for anonymous views.</param>
    /// <param name="dedupKey">The identity surrogate used for deduplication.</param>
    /// <param name="ipAddress">The caller's IP address, or null.</param>
    /// <param name="userAgent">The caller's User-Agent header, or null.</param>
    /// <param name="isCounted">Whether this event incremented the displayed view count.</param>
    /// <returns>A new <see cref="ShortVideoViewEventEntity" />.</returns>
    public static ShortVideoViewEventEntity Create(
        Guid id,
        Guid shortVideoId,
        Guid? userId,
        string dedupKey,
        string? ipAddress,
        string? userAgent,
        bool isCounted
    )
    {
        return new ShortVideoViewEventEntity
        {
            Id = id,
            ShortVideoId = shortVideoId,
            UserId = userId,
            DedupKey = dedupKey,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsCounted = isCounted,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
