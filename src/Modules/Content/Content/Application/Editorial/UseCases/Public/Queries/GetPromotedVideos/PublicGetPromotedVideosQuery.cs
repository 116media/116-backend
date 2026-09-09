using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;

/// <summary>
/// Query for retrieving the list of currently promoted published videos.
/// </summary>
public record PublicGetPromotedVideosQuery() : IQuery<PublicGetPromotedVideosResult>, ICacheableRequest
{
    /// <inheritdoc />
    public string CacheKey => "promoted_videos";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Videos];
}

/// <summary>
/// Result of the <see cref="PublicGetPromotedVideosQuery" /> containing promoted video summaries.
/// </summary>
/// <param name="Videos">The list of promoted video summary DTOs.</param>
public record PublicGetPromotedVideosResult(IReadOnlyList<PublicVideoSummaryDto> Videos);
