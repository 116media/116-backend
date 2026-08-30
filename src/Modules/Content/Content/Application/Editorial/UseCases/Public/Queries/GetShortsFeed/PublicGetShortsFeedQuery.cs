using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;

/// <summary>
/// Query for the randomized short-video feed: a cursor-paginated, seeded pseudo-random
/// ordering of active short videos with stable pagination (no drift across pages).
/// </summary>
/// <param name="Cursor">The opaque feed cursor, or null to start a fresh randomized session.</param>
/// <param name="PageSize">The number of short videos to return.</param>
/// <param name="CurrentUserId">The requesting user id, or null when anonymous; seeds per-user flags.</param>
public record PublicGetShortsFeedQuery(string? Cursor, int PageSize, Guid? CurrentUserId = null)
    : IQuery<PublicGetShortsFeedResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Only the anonymous first page is stored: cursors form an unbounded key space, and an
    /// authenticated response carries per-user interaction flags.
    /// </remarks>
    public bool IsCacheable => CurrentUserId is null && Cursor is null;

    /// <inheritdoc />
    public string CacheKey => $"shorts_feed:first:{PageSize}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Shorts];
}

/// <summary>
/// Result of the <see cref="PublicGetShortsFeedQuery" />.
/// </summary>
/// <param name="Items">The ordered short videos for this page.</param>
/// <param name="NextCursor">The cursor for the next page, or null when the feed is exhausted.</param>
public record PublicGetShortsFeedResult(IReadOnlyList<PublicShortVideoDto> Items, string? NextCursor);
