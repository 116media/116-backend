using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Query for retrieving the most popular published videos, ranked by a weighted engagement
/// score.
/// </summary>
/// <param name="Limit">
/// Maximum number of videos to return. Validated to a small inclusive range.
/// </param>
/// <param name="CategoryId">
/// Optional category filter. When supplied, only videos in that category are ranked.
/// </param>
/// <param name="ExcludeId">
/// Optional video identifier to omit from the result. Used by the video-detail sidebar to
/// drop the video currently being viewed.
/// </param>
public record PublicGetPopularVideosQuery(int Limit, Guid? CategoryId, Guid? ExcludeId)
    : IQuery<PublicGetPopularVideosResult>,
        ICacheableRequest
{
    /// <inheritdoc />
    public string CacheKey =>
        $"popular_videos:{Limit}:{CategoryId?.ToString() ?? "all"}:{ExcludeId?.ToString() ?? "none"}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.PopularVideos];
}

/// <summary>
/// Result of the <see cref="PublicGetPopularVideosQuery" /> containing the ranked video
/// summaries.
/// </summary>
/// <param name="Videos">The popular videos ordered by engagement score descending.</param>
public record PublicGetPopularVideosResult(IReadOnlyList<PublicVideoSummaryDto> Videos);
