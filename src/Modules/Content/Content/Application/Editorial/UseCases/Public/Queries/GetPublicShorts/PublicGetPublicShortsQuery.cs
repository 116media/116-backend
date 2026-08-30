using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;

/// <summary>
/// Query for retrieving a paginated list of active short videos for public consumption.
/// Supports optional search by title.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against title.</param>
/// <param name="CurrentUserId">The requesting user id, or null when anonymous; seeds per-user flags.</param>
public record PublicGetPublicShortsQuery(PaginatedRequest PaginatedRequest, string? Search, Guid? CurrentUserId = null)
    : IQuery<PublicGetPublicShortsResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Only the anonymous projection is stored: an authenticated response carries per-user
    /// interaction flags, and caching it would show one reader another reader's likes.
    /// Free-text search additionally produces an unbounded key space.
    /// </remarks>
    public bool IsCacheable => CurrentUserId is null && string.IsNullOrWhiteSpace(Search);

    /// <inheritdoc />
    public string CacheKey => $"public_shorts:{PaginatedRequest.PageIndex}:{PaginatedRequest.PageSize}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Shorts];
}

/// <summary>
/// Result of the <see cref="PublicGetPublicShortsQuery" /> containing a paginated list of short video DTOs.
/// </summary>
/// <param name="ShortVideos">The paginated result containing short video DTOs.</param>
public record PublicGetPublicShortsResult(PaginatedResult<PublicShortVideoDto> ShortVideos);
