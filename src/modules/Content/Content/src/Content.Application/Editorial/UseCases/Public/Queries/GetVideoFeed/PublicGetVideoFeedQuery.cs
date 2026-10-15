using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Query for retrieving the public video feed: pinned video categories, each with
/// its latest published videos.
/// </summary>
public record PublicGetVideoFeedQuery : IQuery<PublicGetVideoFeedResult>, ICacheableRequest
{
    /// <inheritdoc />
    public string CacheKey => "video_feed";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    /// <remarks>
    /// The feed is grouped by pinned categories, so a category change evicts it alongside
    /// video publish-state changes.
    /// </remarks>
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Videos, ContentCacheTags.Lookups];
}

/// <summary>
/// A single feed entry: one pinned category and its latest published videos.
/// </summary>
/// <param name="Category">The pinned category metadata.</param>
/// <param name="Videos">Up to the per-entry limit of latest published videos in the category, newest first.</param>
public record VideoFeedDto(CategoryDto Category, IReadOnlyList<PublicVideoSummaryDto> Videos);

/// <summary>
/// Result of the <see cref="PublicGetVideoFeedQuery" />.
/// </summary>
/// <param name="Items">
/// Ordered feed entries (most recently pinned category first). Entries whose
/// category has no published videos are omitted.
/// </param>
public record PublicGetVideoFeedResult(IReadOnlyList<VideoFeedDto> Items);
