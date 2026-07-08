using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Handles the <see cref="PublicGetPopularVideosQuery" /> to retrieve the most popular
/// published videos ranked by a weighted engagement score.
/// </summary>
/// <remarks>
/// Results are cached in-process for <see cref="CacheTtl" /> to avoid running the ranking
/// query on every request. Each distinct combination of limit, category, and excluded id
/// produces its own cache entry (e.g. <c>popular_videos_5_all_none</c>). All entries share
/// the eviction token supplied by <see cref="IPopularVideosCacheInvalidator" />, so any
/// engagement (rate, share) or publish-state (publish, archive) mutation cancels the token
/// and evicts every entry at once.
/// </remarks>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileRepository">Repository for resolving thumbnail URLs.</param>
/// <param name="cache">In-process memory cache.</param>
/// <param name="cacheInvalidator">Token source used to evict all popular-videos entries on mutation.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularVideosHandler(
    IVideoRepository videoRepository,
    IFileRepository fileRepository,
    IMemoryCache cache,
    IPopularVideosCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetPopularVideosQuery, PublicGetPopularVideosResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public async Task<PublicGetPopularVideosResult> Handle(
        PublicGetPopularVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        string categoryPart = query.CategoryId?.ToString() ?? "all";
        string excludePart = query.ExcludeId?.ToString() ?? "none";
        string cacheKey = $"popular_videos_{query.Limit}_{categoryPart}_{excludePart}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<VideoSummaryDto>? cached) && cached is not null)
        {
            return new PublicGetPopularVideosResult(Videos: cached);
        }

        IReadOnlyList<VideoEntity> videos = await videoRepository.GetPopularVideosAsync(
            limit: query.Limit,
            excludeId: query.ExcludeId,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<VideoSummaryDto> dtoList = await videos.ToVideoSummaryDtosAsync(
            mapper,
            fileRepository,
            cancellationToken
        );

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheTtl)
            .AddExpirationToken(new CancellationChangeToken(cacheInvalidator.GetEvictionToken()));

        cache.Set(cacheKey, dtoList, options);

        return new PublicGetPopularVideosResult(Videos: dtoList);
    }
}
