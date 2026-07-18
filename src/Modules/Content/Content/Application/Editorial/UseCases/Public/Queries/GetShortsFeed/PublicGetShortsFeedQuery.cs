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
    : IQuery<PublicGetShortsFeedResult>;

/// <summary>
/// Result of the <see cref="PublicGetShortsFeedQuery" />.
/// </summary>
/// <param name="Items">The ordered short videos for this page.</param>
/// <param name="NextCursor">The cursor for the next page, or null when the feed is exhausted.</param>
public record PublicGetShortsFeedResult(IReadOnlyList<ShortVideoDto> Items, string? NextCursor);
