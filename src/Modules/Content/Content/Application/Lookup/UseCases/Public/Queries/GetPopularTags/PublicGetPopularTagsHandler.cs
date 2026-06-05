using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Handles the <see cref="PublicGetPopularTagsQuery" /> to retrieve the most-used tags.
/// </summary>
/// <remarks>
/// Results are cached in-process for <see cref="CacheTtl" /> to avoid running the
/// GROUP BY aggregation query on every request. Popular tags change infrequently,
/// so a short TTL is acceptable and keeps the cache warm under normal traffic.
/// <para>
/// Each distinct combination of <c>Limit</c> and <c>ContentType</c> produces an
/// independent cache entry (e.g. <c>popular_tags_10_article</c>,
/// <c>popular_tags_10_video</c>, <c>popular_tags_all_null</c>). All entries share
/// the same eviction token supplied by <see cref="IPopularTagsCacheInvalidator" />,
/// so any tag-graph mutation cancels the token and instantly evicts every entry
/// regardless of which limit/contentType combinations were cached.
/// </para>
/// </remarks>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="cache">In-process memory cache.</param>
/// <param name="cacheInvalidator">Token source used to evict all popular-tags entries on mutation.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularTagsHandler(
    ILookupRepository lookupRepository,
    IMemoryCache cache,
    IPopularTagsCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetPopularTagsQuery, PublicGetPopularTagsResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public async Task<PublicGetPopularTagsResult> Handle(
        PublicGetPopularTagsQuery query,
        CancellationToken cancellationToken
    )
    {
        string limitPart = query.Limit?.ToString() ?? "all";
        string contentTypePart = query.ContentType?.ToString().ToLowerInvariant() ?? "null";
        string cacheKey = $"popular_tags_{limitPart}_{contentTypePart}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<TagDto>? cached) && cached is not null)
        {
            return new PublicGetPopularTagsResult(Tags: cached);
        }

        IReadOnlyList<TagEntity> tags = await lookupRepository.GetPopularTagsAsync(
            limit: query.Limit,
            contentType: query.ContentType,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<TagDto> dtoList = tags.ToTagDtos(mapper);

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheTtl)
            .AddExpirationToken(new CancellationChangeToken(cacheInvalidator.GetEvictionToken()));

        cache.Set(cacheKey, dtoList, options);

        return new PublicGetPopularTagsResult(Tags: dtoList);
    }
}
